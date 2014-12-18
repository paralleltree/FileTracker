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
    public sealed class FileItem
    {
        public FileInfo Source { get; private set; }
        private string Hash { get; set; }
        private DispatcherDictionary<DateTime, FileInfo> files;
        public ReadOnlyDispatcherCollection<KeyValuePair<DateTime, FileInfo>> SnappedFiles { get; private set; }

        public FileItem(string sourcePath, IEnumerable<KeyValuePair<DateTime, FileInfo>> snapped)
        {
            this.Source = new FileInfo(sourcePath);
            this.Hash = Common.GetHash(Source.Name);
            files = new DispatcherDictionary<DateTime, FileInfo>(DispatcherHelper.UIDispatcher);
            foreach (var f in snapped)
                files.Add(f);

            SnappedFiles = new ReadOnlyDispatcherCollection<KeyValuePair<DateTime, FileInfo>>(files);
        }


        public void Snap()
        {
            Source.Refresh();
            DateTime date = Source.LastWriteTime;

            string snapFolder = Common.GetSnapFolder(Source.Directory.FullName);
            string dest = string.Format("{0}{1}_{2}{3}", snapFolder, Hash, date.ToString(Common.DateFormat), Source.Extension);

            if (!Directory.Exists(snapFolder))
                Common.CreateSnapFolder(Source.Directory.FullName);

            if (files.ContainsKey(date))
            {
                Source.CopyTo(dest, true);
                files[date].Refresh();
            }
            else
            {
                Source.CopyTo(dest);
                files.Add(new KeyValuePair<DateTime, FileInfo>(date, new FileInfo(dest)));
            }
        }

        public void Rename(FileInfo newfile)
        {
            string hash = Common.GetHash(newfile.Name);
            foreach (var f in files)
                f.Value.MoveTo(f.Value.FullName.Replace(this.Hash, hash));

            foreach (DateTime key in files.Keys.ToList())
                files[key] = new FileInfo(files[key].FullName.Replace(this.Hash, hash));

            this.Source = newfile;
            this.Hash = hash;
        }

        public void Clear()
        {
            foreach (var f in files)
                f.Value.Delete();

            files.Clear();
        }

        public void Restore(DateTime date)
        {
            if (!files.ContainsKey(date)) throw new ArgumentException("指定の日時のスナップファイルは検出されませんでした。");
            files[date].CopyTo(string.Format(@"{0}\{1}_{2}{3}", Source.Directory.FullName, Source.Name.Replace(Source.Extension, ""), date.ToString(Common.DateFormat), Source.Extension));
        }
    }
}
