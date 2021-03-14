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
    public class PointSaverLoader : IPointSaverLoader
    {
        private string m_filePath;
        private bool createBackup;

        public PointSaverLoader() : this(Path.Combine(Environment.CurrentDirectory, "path.dat"))
        { }
        public PointSaverLoader(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath 
        {
            get { return m_filePath; }
            init
            {
                if (!File.Exists(value))
                {
                    throw new DirectoryNotFoundException("Wrong file path.");
                }

                m_filePath = value;
            }
        }

        public string FriendlyName => "default";

        public string Description => "Default point loader and saver for files tracks.dat, " +
                                     "tracks2.dat, tracks3.dat, tracks4.dat";

        public string CreatedBy => "MrNails";

        public bool CreateBackup 
        {
            get => createBackup;
            set => createBackup = value;
        }

        public async Task<IEnumerable<GTA_SA_Point>> LoadAsync()
        {
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

        public async Task SaveAsync(IEnumerable<GTA_SA_Point> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            var path = FilePath;
            if (createBackup)
            {
                File.Move(FilePath, FilePath + ".backup");
            }

            using (var fStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, true))
            using (var streamWriter = new StreamWriter(fStream))
            {
                await streamWriter.WriteLineAsync(points.Count().ToString());

                foreach (var item in points)
                {
                    await streamWriter.WriteLineAsync();
                }
            }
        }
    }
}
