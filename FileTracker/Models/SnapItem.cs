using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Livet;

namespace FileTracker.Models
{
    public sealed class SnapItem : NotificationObject
    {
        public FileItem Source { get; private set; }
        public FileInfo SnappedFile { get; private set; }
        public DateTime SnappedTime { get; private set; }
        public string Destination
        {
            get
            {
                var src = Source.Source;
                return string.Format(@"{0}\{1}_{2}{3}",
                    src.Directory.FullName,
                    src.Name.Replace(src.Extension, ""),
                    SnappedTime.ToString(Common.DateFormat),
                    src.Extension);
            }
        }

        public SnapItem(FileItem source, FileInfo snapped, DateTime snappedTime)
        {
            this.Source = source;
            this.SnappedFile = snapped;
            this.SnappedTime = snappedTime;
        }


        public void Remove()
        {
            SnappedFile.Delete();
            Source.SnappedFiles.Remove(SnappedTime);
        }

        public void Restore()
        {
            SnappedFile.CopyTo(Destination, true);
        }
    }
}
