using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Services 
{
    public class DefaultPointLoader : PointLoader
    {
        private string m_filePath;
        private bool m_disposed;

        public DefaultPointLoader(string filePath)
            : base(filePath)
        {  }

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

        public override void Dispose()
        {
            base.Dispose();

            m_disposed = true;

            m_filePath = string.Empty;
        }

        public override Task<IEnumerable<GTA_SA_Point>> LoadAsync()
        {
            return LoadAsync(CancellationToken.None);
        }
        public override async Task<IEnumerable<GTA_SA_Point>> LoadAsync(CancellationToken cancellationToken)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("PointSaverLoader");
            }

            if (!File.Exists(FilePath))
            {
                throw new DirectoryNotFoundException("Wrong file path.");
            }

            GTA_SA_Point[]? points = null;
            string filePath = m_filePath;
            char[] splitCharacters = new char[] { ' ' };
            int lineNumber = 0;

            using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
            using (var streamReader = new StreamReader(fStream))
            {
                int pointsCount;
                lineNumber++;
                if (int.TryParse(streamReader.ReadLine(), out pointsCount))
                {
                    points = new GTA_SA_Point[pointsCount];

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
                            double x, y, z;
                            int isStop;

                            if (double.TryParse(currentLine[0], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out x) &&
                                double.TryParse(currentLine[1], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out y) &&
                                double.TryParse(currentLine[2], NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out z) &&
                                int.TryParse(currentLine[3], out isStop))
                            {

                                points[lineNumber - 2] = new GTA_SA_Point(x, y, z, isStop == 1);
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
    }
}
