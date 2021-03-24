using System;

namespace GTA_SA_PathsRedactor.Core
{
    [Serializable]
    public sealed class PointsLoadingException : Exception
    {
        private string m_pointsFileName;
        private int m_fileErrorLine;

        public PointsLoadingException(string pointsFileName, int fileErrorLine)
            : this (pointsFileName, fileErrorLine, $"An error occured in file {pointsFileName} at line {fileErrorLine}")
        { }
        public PointsLoadingException(string pointsFileName, int fileErrorLine, string message) : base(message)
        {
            m_pointsFileName = pointsFileName;
            m_fileErrorLine = fileErrorLine;
        }
        public PointsLoadingException(string pointsFileName, int fileErrorLine, string message, Exception inner) : base(message, inner) 
        {
            m_pointsFileName = pointsFileName;
            m_fileErrorLine = fileErrorLine;
        }
        private PointsLoadingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) 
        {
            info.AddValue("m_pointsFile", m_pointsFileName);
            info.AddValue("m_fileErrorLine", m_fileErrorLine);
        }

        public string PointsFileName { get => m_pointsFileName; }
        public int FileErrorLine { get => m_fileErrorLine; }
    }
}
