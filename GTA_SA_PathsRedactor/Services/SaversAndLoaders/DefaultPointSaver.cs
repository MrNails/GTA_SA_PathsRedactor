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
        private string m_fileName;
        private bool m_createBackup;
        private bool m_disposed;

        public DefaultPointSaver() : this(string.Empty)
        { }
        public DefaultPointSaver(string fileName)
        {
            FileName = fileName;
        }

        public override string FileName
        {
            get { return m_fileName; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (m_disposed)
                    throw new ObjectDisposedException("PointSaverLoader");

                m_fileName = value;
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

            var tempFilePase = CreateTempFilePath(FileName);
            if (File.Exists(tempFilePase))
                File.Delete(tempFilePase);

            m_disposed = true;

            m_fileName = string.Empty;
            m_createBackup = false;
        }

        public override Task SaveAsync(IEnumerable<WorldPoint> points)
        {
            return SaveAsync(points, CancellationToken.None);
        }
        public override async Task SaveAsync(IEnumerable<WorldPoint> points, CancellationToken cancellationToken)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("PointSaverLoader");
            }

            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            StringBuilder stringBuilder = new StringBuilder();

            var tempFilePath = CreateTempFilePath(FileName);

            using var fStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write,
                                               FileShare.None, 4096, true);

            var fileInfo = new FileInfo(tempFilePath);
            var oldFileAttributes = fileInfo.Attributes;
            fileInfo.Attributes = FileAttributes.Hidden;

            if (m_createBackup && File.Exists(FileName))
                SetFileAsBackup(FileName);

            using (var streamWriter = new StreamWriter(fStream))
            {
                await streamWriter.WriteLineAsync(points.Count().ToString());

                foreach (var point in points)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    stringBuilder.Clear();
                    stringBuilder.Append(point.X.ToString(CultureInfo.InvariantCulture))
                                 .Append(' ')
                                 .Append(point.Y.ToString(CultureInfo.InvariantCulture))
                                 .Append(' ')
                                 .Append(point.Y.ToString(CultureInfo.InvariantCulture))
                                 .Append(point.IsStopPoint ? " 1" : " 0");

                    

                    await streamWriter.WriteLineAsync(stringBuilder.ToString());
                }
            }

            fileInfo.Attributes = oldFileAttributes;

            File.Delete(FileName);
            File.Move(tempFilePath, FileName);
            File.Delete(tempFilePath);
        }

        private string CreateTempFilePath(string filePath)
        {
            StringBuilder pathBuilder = new StringBuilder(filePath);

            pathBuilder.Insert(filePath.LastIndexOf('\\') + 1, "_$");
            pathBuilder.Insert(filePath.LastIndexOf('.') + 2, "_$");

            return pathBuilder.ToString();
        }

        private void SetFileAsBackup(string filePath)
        {
            var backupFilePath = FileName + ".backup";

            if (!File.Exists(backupFilePath))
                File.Move(filePath, backupFilePath);
        }
    }
}