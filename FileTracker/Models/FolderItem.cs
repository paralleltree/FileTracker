using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Livet;

namespace FileTracker.Models
{
    class FolderItem : IDisposable
    {
        private FileSystemWatcher Watcher { get; set; }

        public string Path
        {
            get { return Watcher.Path; }
            set { Watcher.Path = value.Substring(value.Length - 1, 1) == @"\" ? value : value + @"\"; }
        }

        public bool IsWatching
        {
            get { return Watcher.EnableRaisingEvents; }
            set { Watcher.EnableRaisingEvents = value; }
        }

        private DispatcherDictionary<string, FileItem> files;
        public ReadOnlyDispatcherCollection<KeyValuePair<string, FileItem>> TrackingFiles { get; private set; }

        public FolderItem(string path)
        {
            files = new DispatcherDictionary<string, FileItem>(DispatcherHelper.UIDispatcher);

            string snapFolder = Common.GetSnapFolder(path);
            if (Directory.Exists(snapFolder))
            {
                // スナップフォルダからスナップファイルを検索する
                var snapped = Directory.GetFiles(snapFolder)
                    .Select(p => new { Path = p, Match = Regex.Match(p, @"\\(?<hash>[a-z0-9]+)_(?<date>\d{14})(\..+)?\Z") })
                    .Where(p => p.Match.Success)
                    .Select(p => new
                    {
                        Hash = p.Match.Groups["hash"].Value,
                        Date = DateTime.ParseExact(p.Match.Groups["date"].Value, Common.DateFormat, null),
                        Path = p.Path
                    })
                    .ToList();

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
                    string hash = Common.GetHash(file.Substring(file.LastIndexOf(@"\") + 1, file.LastIndexOf(@"\")));
                    if (dic.ContainsKey(hash))
                        files.Add(hash, new FileItem(file, dic[hash]));
                }
            }

            TrackingFiles = new ReadOnlyDispatcherCollection<KeyValuePair<string, FileItem>>(files);

            Watcher = new FileSystemWatcher(path)
            {
                EnableRaisingEvents = true
            };
            Watcher.Created += OnCreated;
            Watcher.Changed += OnChanged;
            Watcher.Deleted += OnDeleted;
            Watcher.Renamed += OnRenamed;
            Watcher.Error += OnError;
        }


        private void OnCreated(object sender, FileSystemEventArgs e)
        {

        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {

        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {

        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {

        }

        private void OnError(object sender, ErrorEventArgs e)
        {

        }


        public void Dispose()
        {
            if (this.Watcher != null)
            {
                Watcher.Created -= OnCreated;
                Watcher.Changed -= OnChanged;
                Watcher.Deleted -= OnDeleted;
                Watcher.Renamed -= OnRenamed;
                Watcher.Error -= OnError;
                Watcher.Dispose();
            }
        }
    }
}
