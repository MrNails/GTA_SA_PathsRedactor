using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Services
{
    public sealed class PathSelectionArgs
    {
        private int m_oldIndex;
        private int m_newIndex;

        private ViewModel.PathEditorViewModel? m_pathEditor;

        public PathSelectionArgs(ViewModel.PathEditorViewModel? pathEditor, int oldIndex, int newIndex)
        {
            m_pathEditor = pathEditor;
            m_newIndex = newIndex;
            m_oldIndex = oldIndex;
        }

        public int OldIndex => m_oldIndex;
        public int NewIndex => m_newIndex;
        public ViewModel.PathEditorViewModel? Path => m_pathEditor;
    }
}
