using System;
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
        public event EventHandler<ErrorEventArgs> WatcherDisabled;


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


        public FolderItem(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("指定のディレクトリは見つかりませんでした。");

            TrackingFiles = new DispatcherDictionary<string, FileItem>(DispatcherHelper.UIDispatcher);

            string snapFolder = Common.GetSnapFolder(path);
            if (Directory.Exists(snapFolder))
            {
                // スナップフォルダからスナップファイルを検索する
                var snapped = Directory.GetFiles(snapFolder)
                    .Select(p => new { Path = p, Match = Regex.Match(p, @"\\(?<hash>[a-f0-9]+)_(?<date>\d{14})(\..+)?\Z") })
                    .Where(p => p.Match.Success)
                    .Select(p => new
                    {
                        Hash = p.Match.Groups["hash"].Value,
                        Date = DateTime.ParseExact(p.Match.Groups["date"].Value, Common.DateFormat, null),
                        Path = p.Path
                    });

                // 同一ファイルごとに辞書に追加
                var dic = new Dictionary<string, Dictionary<DateTime, FileInfo>>();
                foreach (var item in snapped)
                {
                    if (!dic.ContainsKey(item.Hash))
                        dic.Add(item.Hash, new Dictionary<DateTime, FileInfo>());
                    dic[item.Hash].Add(item.Date, new FileInfo(item.Path));
                }

                // 現在のフォルダ内のファイルをチェック
                // スナップファイルが存在すればFileItemを追加
                foreach (string file in Directory.GetFiles(path))
                {
                    var info = new FileInfo(file);
                    string hash = Common.GetHash(info.Name);
                    if (dic.ContainsKey(hash))
                        TrackingFiles.Add(hash, new FileItem(info, dic[hash]));
                    dic.Remove(hash);
                }

                // ソースを失ったスナップファイルを削除
                foreach (var item in dic)
                {
                    foreach (var file in item.Value)
                        file.Value.Delete();
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
                TrackingFiles.Add(hash, new FileItem(file, Enumerable.Empty<KeyValuePair<DateTime, FileInfo>>()));
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
                if (WatcherDisabled != null)
                    WatcherDisabled(this, e);
            }
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
