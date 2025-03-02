namespace GTA_SA_PathsRedactor.Models.EventArguments
{
    public sealed class PathSelectionArgs
    {
        public PathSelectionArgs(ViewModel.PathEditorViewModel? pathEditor, int oldIndex, int newIndex)
        {
            Path = pathEditor;
            NewIndex = newIndex;
            OldIndex = oldIndex;
        }

        public int OldIndex { get; }
        public int NewIndex { get; }
        public ViewModel.PathEditorViewModel? Path { get; }
    }
}
