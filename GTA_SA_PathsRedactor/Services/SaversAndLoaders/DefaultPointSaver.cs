using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Services.SaversAndLoaders
{
    public sealed class DefaultPointSaver : IPointSaver
    {
        private string _fileName = string.Empty;
        private bool _createBackup;
        private bool _disposed;

        public string FileName
        {
            get => _fileName;
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(DefaultPointSaver));

                _fileName = string.IsNullOrWhiteSpace(value) 
                    ? throw new ArgumentNullException(nameof(value))
                    : value;
            }
        }

        public bool CreateBackup
        {
            get => _createBackup;
            set
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(DefaultPointSaver));
                }

                _createBackup = value;
            }
        }

        public void Dispose()
        {
            var tempFilePath = CreateTempFilePath(FileName);
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            _disposed = true;

            _fileName = string.Empty;
            _createBackup = false;
        }
        
        public async Task SaveAsync(IEnumerable<WorldPoint> points, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("PointSaverLoader");
            }

            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            StringBuilder stringBuilder = new StringBuilder();

            var tempFilePath = CreateTempFilePath(FileName);

            using var fStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write,
                                               FileShare.None, 4096, true);

            var fileInfo = new FileInfo(tempFilePath);
            var oldFileAttributes = fileInfo.Attributes;
            fileInfo.Attributes = FileAttributes.Hidden;

            if (_createBackup && File.Exists(FileName))
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