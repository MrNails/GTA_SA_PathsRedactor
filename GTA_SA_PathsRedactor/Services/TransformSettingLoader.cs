using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace GTA_SA_PathsRedactor.Services
{
    public class TransformSettingLoader
    {
        private string m_filePath;

        public TransformSettingLoader(string pathName)
        {
            FilePath = pathName;
        }

        public string FilePath
        {
            get { return m_filePath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("File path can't be empty.");
                }

                m_filePath = value;
            }
        }

        public void SaveSettings(IEnumerable<PointTransformationData> pointTransformationDatas)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            if (!File.Exists(m_filePath))
            {
                File.Create(m_filePath).Close();
            }

            using (FileStream fileStream = new FileStream(m_filePath, FileMode.Truncate, FileAccess.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    jsonSerializer.Serialize(streamWriter, pointTransformationDatas);
                }
            }
        }
        public List<PointTransformationData> LoadSettings()
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            if (!File.Exists(m_filePath))
            {
                throw new FileNotFoundException();
            }

            using (FileStream fileStream = new FileStream(m_filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    return (List<PointTransformationData>)jsonSerializer.Deserialize(streamReader, typeof(List<PointTransformationData>));
                }
            }
        }
    }
}
