using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using Livet;

namespace FileTracker.Models
{
    public sealed class FileItem : NotificationObject
    {
        private FileInfo _source;
        public FileInfo Source
        {
            get { return _source; }
            private set
            {
                if (_source == value) return;
                _source = value;
                RaisePropertyChanged();
            }
        }
        private string Hash { get; set; }
        public DispatcherDictionary<DateTime, SnapItem> SnappedFiles { get; private set; }


        public FileItem(FileInfo source)
            : this(source, Enumerable.Empty<KeyValuePair<DateTime, SnapItem>>())
        {
        }

        public FileItem(FileInfo source, IEnumerable<KeyValuePair<DateTime, SnapItem>> snapped)
        {
            this.Source = source;
            this.Hash = Common.GetHash(Source.Name);
            SnappedFiles = new DispatcherDictionary<DateTime, SnapItem>(DispatcherHelper.UIDispatcher);
            foreach (var f in snapped)
                SnappedFiles.Add(f);
        }


        public void Snap()
        {
            Source.Refresh();
            DateTime date = Source.LastWriteTime;

            string snapFolder = Common.GetSnapFolder(Source.Directory.FullName);
            string dest = string.Format("{0}{1}_{2}{3}", snapFolder, Hash, date.ToUniversalTime().Ticks, Source.Extension);

            if (!Directory.Exists(snapFolder))
                Common.CreateSnapFolder(Source.Directory.FullName);

            if (SnappedFiles.ContainsKey(date))
            {
                Source.CopyTo(dest, true);
                SnappedFiles[date].SnappedFile.Refresh();
            }
            else
            {
                Source.CopyTo(dest);
                SnappedFiles.Add(date, new SnapItem(this, new FileInfo(dest), date));
            }

            RaisePropertyChanged("Source");
        }

        public void Rename(FileInfo newfile)
        {
            string hash = Common.GetHash(newfile.Name);
            foreach (var f in SnappedFiles)
                f.Value.SnappedFile.MoveTo(f.Value.SnappedFile.FullName.Replace(this.Hash, hash));

            this.Source = newfile;
            this.Hash = hash;
        }

        public void Clear()
        {
            var list = SnappedFiles.ToList();
            foreach (var item in list)
                item.Value.Remove();
        }
    }
}
