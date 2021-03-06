﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Livet;

namespace FileTracker.Models
{
    public sealed class FolderItem : NotificationObject, IDisposable
    {
        private Model Model { get; set; }
        private IDisposable FileChangedListener { get; set; }
        private FileSystemWatcher Watcher { get; set; }

        public string Path
        {
            get { return Watcher.Path; }
            set
            {
                Watcher.Path = value.Substring(value.Length - 1, 1) == @"\" ? value : value + @"\";
                RaisePropertyChanged();
            }
        }

        public bool IsWatching
        {
            get { return Watcher.EnableRaisingEvents; }
            set
            {
                Watcher.EnableRaisingEvents = value;
                RaisePropertyChanged();
            }
        }

        public DispatcherDictionary<string, FileItem> TrackingFiles { get; private set; }


        public FolderItem(Model model, string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("指定のディレクトリは見つかりませんでした。");

            this.Model = model;
            TrackingFiles = new DispatcherDictionary<string, FileItem>(DispatcherHelper.UIDispatcher);

            string snapFolder = Common.GetSnapFolder(path);
            if (Directory.Exists(snapFolder))
            {
                // スナップフォルダからスナップファイルを検索する
                var snapped = Directory.GetFiles(snapFolder)
                    .Select(p => new { Path = p, Match = Regex.Match(p, @"\\(?<hash>[a-f0-9]+)_(?<tick>\d+)(\..+)?\Z") })
                    .Where(p => p.Match.Success)
                    .Select(p => new
                    {
                        Hash = p.Match.Groups["hash"].Value,
                        Date = new DateTime(long.Parse(p.Match.Groups["tick"].Value), DateTimeKind.Utc).ToLocalTime(),
                        Path = p.Path
                    });

                // 同一ファイルごとに辞書に追加
                var dic = new Dictionary<string, Dictionary<DateTime, string>>();
                foreach (var item in snapped)
                {
                    if (!dic.ContainsKey(item.Hash))
                        dic.Add(item.Hash, new Dictionary<DateTime, string>());
                    dic[item.Hash].Add(item.Date, item.Path);
                }

                // 現在のフォルダ内のファイルをチェック
                // スナップファイルが存在すればFileItemを追加
                foreach (string file in Directory.GetFiles(path))
                {
                    var src = new FileInfo(file);
                    string hash = Common.GetHash(src.Name);
                    if (dic.ContainsKey(hash))
                    {
                        var item = new FileItem(src);
                        foreach (var snap in dic[hash])
                            item.SnappedFiles.Add(snap.Key, new SnapItem(item, new FileInfo(snap.Value), snap.Key));

                        TrackingFiles.Add(hash, item);
                    }
                    dic.Remove(hash);
                }

                // ソースを失ったスナップファイルを削除
                foreach (var item in dic)
                {
                    foreach (var file in item.Value)
                        File.Delete(file.Value);
                }
            }


            Watcher = CreateWatcher(path);

            FileChangedListener = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => Watcher.Changed += h,
                h => Watcher.Changed -= h)
                .GroupBy(p => p.EventArgs.Name)
                .Subscribe(p =>
                {
                    p.Throttle(TimeSpan.FromSeconds(1))
                        .Subscribe(q => OnChanged(q.Sender, q.EventArgs));
                });

            //Watcher.Changed += OnChanged;
            Watcher.Deleted += OnDeleted;
            Watcher.Renamed += OnRenamed;
            Watcher.Error += OnError;
        }


        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            // ディレクトリ作成イベントも流れてくる
            var file = new FileInfo(e.FullPath);
            if (file.Attributes.HasFlag(FileAttributes.Directory)) return;

            string hash = Common.GetHash(e.Name);
            if (!TrackingFiles.ContainsKey(hash))
                TrackingFiles.Add(hash, new FileItem(file));
            TrackingFiles[hash].Snap();
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            string hash = Common.GetHash(e.Name);
            if (!TrackingFiles.ContainsKey(hash)) return;

            TrackingFiles[hash].Clear();
            TrackingFiles.Remove(hash);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            string hash = Common.GetHash(e.OldName);
            if (!TrackingFiles.ContainsKey(hash)) return;

            var item = TrackingFiles[hash];
            item.Rename(new FileInfo(e.FullPath));
            TrackingFiles.Remove(hash);
            TrackingFiles.Add(Common.GetHash(e.Name), item);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            if (ex != null)
            {
                System.Diagnostics.Debug.WriteLine("WatcherError: {0}\n{1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
            }

            string path = Watcher.Path;
            Dispose();
            try
            {
                Watcher = CreateWatcher(path);
            }
            catch (Exception)
            {
                RemoveFromCollection();
            }
        }


        public void Clear()
        {
            string snapFolder = Common.GetSnapFolder(Path);
            if (Directory.Exists(snapFolder))
                Directory.Delete(snapFolder, true);

            TrackingFiles.Clear();
        }

        public void AddToCollection()
        {
            if (Model.TrackingFolders.Contains(this)) throw new InvalidOperationException("既にコレクションに追加されています。");
            Model.TrackingFolders.Add(this);
        }

        public void RemoveFromCollection()
        {
            Model.TrackingFolders.Remove(this);
        }

        public void Dispose()
        {
            FileChangedListener.Dispose();
            if (this.Watcher != null)
            {
                //Watcher.Changed -= OnChanged;
                Watcher.Deleted -= OnDeleted;
                Watcher.Renamed -= OnRenamed;
                Watcher.Error -= OnError;
                Watcher.Dispose();
            }
        }

        private static FileSystemWatcher CreateWatcher(string path)
        {
            return new FileSystemWatcher(path)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };
        }
    }
}
