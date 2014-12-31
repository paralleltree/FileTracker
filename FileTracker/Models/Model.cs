using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Livet;
using Newtonsoft.Json;

namespace FileTracker.Models
{
    public sealed class Model : NotificationObject, IDisposable
    {
        private static Model _model;
        public static Model Instance
        {
            get
            {
                if (_model == null) _model = new Model();
                return _model;
            }
        }

        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        private readonly string TargetsPath = "targets.json";

        public DispatcherCollection<FolderItem> TrackingFolders { get; private set; }


        private Model()
        {
            TrackingFolders = new DispatcherCollection<FolderItem>(DispatcherHelper.UIDispatcher);
            Initialize();
        }

        public void Initialize()
        {
            if (File.Exists(TargetsPath))
            {
                var paths = JsonConvert.DeserializeObject<IEnumerable<string>>(File.ReadAllText(TargetsPath, Common.Encoder));
                foreach (string path in paths)
                {
                    if (Directory.Exists(path))
                        TrackingFolders.Add(new FolderItem(path));
                }
            }
        }


        public void AddFolder(string path)
        {
            path = Regex.Replace(path, @"\\{2,}\Z", "");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException("存在しないフォルダです。");

            if (TrackingFolders.Any(p => string.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("既に登録されたフォルダです。", "path");

            TrackingFolders.Add(new FolderItem(path));
        }

        public void RemoveFolder(FolderItem item)
        {
            TrackingFolders.Remove(item);
        }


        public void Dispose()
        {
            foreach (var f in TrackingFolders)
                f.Dispose();

            File.WriteAllText(TargetsPath, JsonConvert.SerializeObject(TrackingFolders.Select(p => p.Path)));

            _model = null;
        }
    }
}
