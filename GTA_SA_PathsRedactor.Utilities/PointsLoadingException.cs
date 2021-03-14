using System;

namespace GTA_SA_PathsRedactor.Core
{
    [Serializable]
    public sealed class PointsLoadingException : Exception
    {
        private string m_pointsFile;
        private int m_fileErrorLine;

        public PointsLoadingException(string pointsFile, int fileErrorLine)
            : this (pointsFile, fileErrorLine, $"An error occured in file {pointsFile} at line {fileErrorLine}")
        { }
        public PointsLoadingException(string pointsFile, int fileErrorLine, string message) : base(message)
        {
            m_pointsFile = pointsFile;
            m_fileErrorLine = fileErrorLine;
        }
        public PointsLoadingException(string pointsFile, int fileErrorLine, string message, Exception inner) : base(message, inner) 
        {
            m_pointsFile = pointsFile;
            m_fileErrorLine = fileErrorLine;
        }
        private PointsLoadingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) 
        {
            info.AddValue("m_pointsFile", m_pointsFile);
            info.AddValue("m_fileErrorLine", m_fileErrorLine);
        }

        public string PointsFile { get => m_pointsFile; }
        public int FileErrorLine { get => m_fileErrorLine; }
    }
}
