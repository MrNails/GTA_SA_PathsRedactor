using System.IO;
using Newtonsoft.Json;

namespace GTA_SA_PathsRedactor.Services
{
    public class TransformSettingSaverLoader
    {
        private string m_filePath;

        public TransformSettingSaverLoader(string pathName)
        {
            FilePath = pathName;
        }

        public string FilePath
        {
            get { return m_filePath; }
            set
            {
                if (!File.Exists(value))
                {
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
