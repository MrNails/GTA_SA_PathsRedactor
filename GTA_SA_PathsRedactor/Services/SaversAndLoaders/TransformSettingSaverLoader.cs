using System;
using System.IO;
using Newtonsoft.Json;

namespace GTA_SA_PathsRedactor.Services
{
    public class TransformSettingSaverLoader
    {
        private string m_filePath;
        private bool m_save;

        public TransformSettingSaverLoader(string pathName, bool save = false)
        {
            m_save = save;
            FilePath = pathName;
        }

        public string FilePath
        {
            get { return m_filePath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("File name cannot be empty.", nameof(value));

                if (!File.Exists(value))
                {
                    if (m_save)
                        File.Create(value).Close();
                    else
                        throw new FileNotFoundException("Unable to find file: " + value);
                }

                m_filePath = value;
            }
        }

        public void SaveSettings(PointTransformationData pointTransformationData)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            using (FileStream fileStream = new FileStream(m_filePath, FileMode.Truncate, FileAccess.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    jsonSerializer.Serialize(streamWriter, pointTransformationData);
                }
            }
        }
        public PointTransformationData LoadSettings()
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            using (FileStream fileStream = new FileStream(m_filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    return (PointTransformationData)jsonSerializer.Deserialize(streamReader, typeof(PointTransformationData));
                }
            }
        }
    }
}
