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

            WorldPoint[]? points = null;
            string filePath = _fileName;
            char[] splitCharacters = new char[] { ' ' };
            int lineNumber = 0;

            using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
            using (var streamReader = new StreamReader(fStream))
            {
                int pointsCount;
                lineNumber++;
                if (int.TryParse(streamReader.ReadLine(), out pointsCount))
                {
                    points = new WorldPoint[pointsCount];

                    while (!streamReader.EndOfStream)
                    {
                        var currentLine = (await streamReader.ReadLineAsync()).Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);
                        lineNumber++;

                        if (currentLine.Length == 0)
                        {
                            continue;
                        }
                        else if (currentLine.Length != 4)
                        {
                            throw new PointsLoadingException(filePath, lineNumber);
                        }
                        else if (lineNumber - 2 >= points.Length)
                        {
                            break;
                        }
                        else
                        {
                            float x, y, z;
                            int isStop;

                            if (float.TryParse(currentLine[0], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out x) &&
                                float.TryParse(currentLine[1], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out y) &&
                                float.TryParse(currentLine[2], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out z) &&
                                int.TryParse(currentLine[3], out isStop))
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
                else
                {
                    throw new PointsLoadingException(filePath, lineNumber);
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
