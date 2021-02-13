using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Services
{
    public class PointLoader
    {
        private string m_gameFolderPath;
        private object m_pathType;

        public PointLoader(string gameFolderPath, object pathType = null)
        {
            GameFolderPath = gameFolderPath;
            PathType = pathType;
        }

        public string GameFolderPath 
        {
            get { return m_gameFolderPath; }
            init
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_gameFolderPath = value.Remove(value.LastIndexOf('\\'));

                if (!File.Exists(m_gameFolderPath + @"\data\Paths\tracks.dat"))
                {
                    throw new DirectoryNotFoundException("Wrong game directory path.");
                }
            }
        }

        public object PathType
        {
            get { return m_pathType; }
            set { m_pathType = value; }
        }

        public Task<Models.GTA_SA_Point[]> LoadPointsAsync()
        {
            return LoadPointsAsync(CancellationToken.None);
        }
        public async Task<Models.GTA_SA_Point[]> LoadPointsAsync(CancellationToken cancellationToken)
        {
            Models.GTA_SA_Point[] points = null;
            string filePath = m_gameFolderPath + @"\data\Paths\tracks2.dat";
            char[] splitCharacters = new char[] { ' ' }; 
            int lineNumber = 0;

            using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
            using (var streamReader = new StreamReader(fStream))
            {
                int pointsCount;
                lineNumber++;
                if (int.TryParse(streamReader.ReadLine(), out pointsCount))
                {
                    points = new Models.GTA_SA_Point[pointsCount];

                    while (!streamReader.EndOfStream)
                    {
                        var currentLine = (await streamReader.ReadLineAsync()).Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);
                        lineNumber++;

                        if (currentLine.Length != 4)
                        {
                            throw new PointsLoadingException(filePath, lineNumber);
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
                                points[lineNumber - 2] = new Models.GTA_SA_Point(x, y, z, isStop == 1);
                            }
                            else
                            {
                                throw new PointsLoadingException(filePath, lineNumber);
                            }
                        }

                        cancellationToken.ThrowIfCancellationRequested();
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
