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
        public DispatcherDictionary<DateTime, FileInfo> SnappedFiles { get; private set; }

        public FileItem(FileInfo source, IEnumerable<KeyValuePair<DateTime, FileInfo>> snapped)
        {
            this.Source = source;
            this.Hash = Common.GetHash(Source.Name);
            SnappedFiles = new DispatcherDictionary<DateTime, FileInfo>(DispatcherHelper.UIDispatcher);
            foreach (var f in snapped)
                SnappedFiles.Add(f);
        }


        public void Snap()
        {
            Source.Refresh();
            DateTime date = Source.LastWriteTime;

            string snapFolder = Common.GetSnapFolder(Source.Directory.FullName);
            string dest = string.Format("{0}{1}_{2}{3}", snapFolder, Hash, date.ToString(Common.DateFormat), Source.Extension);

            if (!Directory.Exists(snapFolder))
                Common.CreateSnapFolder(Source.Directory.FullName);

            if (SnappedFiles.ContainsKey(date))
            {
                Source.CopyTo(dest, true);
                SnappedFiles[date].Refresh();
            }
            else
            {
                Source.CopyTo(dest);
                SnappedFiles.Add(new KeyValuePair<DateTime, FileInfo>(date, new FileInfo(dest)));
            }
        }

        public void Rename(FileInfo newfile)
        {
            string hash = Common.GetHash(newfile.Name);
            foreach (var f in SnappedFiles)
                f.Value.MoveTo(f.Value.FullName.Replace(this.Hash, hash));

            foreach (DateTime key in SnappedFiles.Keys.ToList())
                SnappedFiles[key] = new FileInfo(SnappedFiles[key].FullName.Replace(this.Hash, hash));

            this.Source = newfile;
            this.Hash = hash;
        }

        public void Clear()
        {
            foreach (var f in SnappedFiles)
                f.Value.Delete();

            SnappedFiles.Clear();
        }

        public void Remove(DateTime date)
        {
            if (!SnappedFiles.ContainsKey(date)) throw new ArgumentException("指定の日時のスナップファイルは検出されませんでした。");
            SnappedFiles[date].Delete();
            SnappedFiles.Remove(date);
        }

        public void Restore(DateTime date)
        {
            if (!SnappedFiles.ContainsKey(date)) throw new ArgumentException("指定の日時のスナップファイルは検出されませんでした。");
            SnappedFiles[date].CopyTo(string.Format(@"{0}\{1}_{2}{3}", Source.Directory.FullName, Source.Name.Replace(Source.Extension, ""), date.ToString(Common.DateFormat), Source.Extension));
        }
    }
}
