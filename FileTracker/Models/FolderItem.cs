using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
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
            set { Watcher.Path = value; }
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
            Watcher = new FileSystemWatcher(path)
            {
                EnableRaisingEvents = true
            };
            Watcher.Created += OnCreated;
            Watcher.Changed += OnChanged;
            Watcher.Deleted += OnDeleted;
            Watcher.Renamed += OnRenamed;
            Watcher.Error += OnError;

            files = new DispatcherDictionary<string, FileItem>(DispatcherHelper.UIDispatcher);
            TrackingFiles = new ReadOnlyDispatcherCollection<KeyValuePair<string, FileItem>>(files);
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
