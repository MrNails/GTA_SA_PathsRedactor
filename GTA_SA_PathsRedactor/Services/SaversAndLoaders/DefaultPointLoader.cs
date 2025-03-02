using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Services.SaversAndLoaders
{
    public sealed class DefaultPointLoader : IPointLoader
    {
        private string _fileName = string.Empty;
        private bool _disposed;

        public string FileName
        {
            get => _fileName;
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(DefaultPointLoader));

                _fileName = string.IsNullOrWhiteSpace(value)
                    ? throw new ArgumentNullException(nameof(value))
                    : value;
            }
        }

        public async Task<IEnumerable<WorldPoint>> LoadAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DefaultPointLoader));
            }

            if (!File.Exists(FileName))
            {
                throw new FileNotFoundException("Wrong file path.");
            }

            var points = Array.Empty<WorldPoint>();
            var filePath = _fileName;
            var lineNumber = 0;

            using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
            using (var streamReader = new StreamReader(fStream))
            {
                lineNumber++;
                if (!int.TryParse(await streamReader.ReadLineAsync(), out var pointsCount))
                {
                    throw new PointsLoadingException(filePath, lineNumber);
                }

                points = new WorldPoint[pointsCount];

                while (!streamReader.EndOfStream)
                {
                    lineNumber++;
                    var line = await streamReader.ReadLineAsync();

                    if (line is null)
                        throw new PointsLoadingException(filePath, lineNumber);

                    var splittedLine = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (splittedLine.Length == 0)
                        continue;
                    
                    if (splittedLine.Length != 4)
                        throw new PointsLoadingException(filePath, lineNumber);
                    else if (lineNumber - 2 >= points.Length)
                        break;
                    else
                    {
                        if (float.TryParse(splittedLine[0], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out var x) &&
                            float.TryParse(splittedLine[1], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out var y) &&
                            float.TryParse(splittedLine[2], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out var z) &&
                            int.TryParse(splittedLine[3], out var isStop))
                        {

                            points[lineNumber - 2] = new WorldPoint(x, y, z, isStop == 1);
                        }
                        else
                        {
                            throw new PointsLoadingException(filePath, lineNumber);
                        }
                    }
                }

            }

            return points;
        }

        public void Dispose()
        {
            _disposed = true;
            _fileName = string.Empty;
        }
    }
}
