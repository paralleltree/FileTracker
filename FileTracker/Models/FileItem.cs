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
    class FileItem
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
    }
}
