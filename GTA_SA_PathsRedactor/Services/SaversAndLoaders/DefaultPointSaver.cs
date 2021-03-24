using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Services
{
    public class DefaultPointSaver : PointSaver
    {
        private string m_filePath;
        private bool m_createBackup;
        private bool m_disposed;

        //static DefaultPointSaverLoader()
        //{
        //    FriendlyName = "default";
        //    Description = "Default point loader and saver for files tracks.dat, " +
        //                  "tracks2.dat, tracks3.dat, tracks4.dat";
        //    CreatedBy = "MrNails";
        //}

        public DefaultPointSaver(string filePath)
            : base(filePath)
        { }

        public override string FilePath
        {
            get { return m_filePath; }
            set
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException("PointSaverLoader");
                }

                m_filePath = value;
            }
        }

        public override bool CreateBackup
        {
            get => m_createBackup;
            set
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException("PointSaverLoader");
                }

                m_createBackup = value;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            m_disposed = true;

            m_filePath = string.Empty;
            m_createBackup = false;
        }

        public override Task SaveAsync(IEnumerable<GTA_SA_Point> points)
        {
            return SaveAsync(points, CancellationToken.None);
        }
        public override async Task SaveAsync(IEnumerable<GTA_SA_Point> points, CancellationToken cancellationToken)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("PointSaverLoader");
            }

            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            FileMode fileMode = FileMode.Create;

            if (m_createBackup && File.Exists(FilePath))
            {
                var backupFilePath = FilePath + ".backup";

                if (File.Exists(backupFilePath))
                    fileMode = FileMode.Truncate;
                else
                    File.Move(FilePath, backupFilePath);
            }

            StringBuilder stringBuilder = new StringBuilder();

            using (var fStream = new FileStream(FilePath, fileMode, FileAccess.Write, FileShare.None, 4096, true))
            using (var streamWriter = new StreamWriter(fStream))
            {
                await streamWriter.WriteLineAsync(points.Count().ToString());

                foreach (var point in points)
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(point.X.ToString(CultureInfo.InvariantCulture))
                                 .Append(' ')
                                 .Append(point.Y.ToString(CultureInfo.InvariantCulture))
                                 .Append(' ')
                                 .Append(point.Y.ToString(CultureInfo.InvariantCulture))
                                 .Append(point.IsStopPoint ? " 1" : " 0");

                    await streamWriter.WriteLineAsync(stringBuilder.ToString());
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}