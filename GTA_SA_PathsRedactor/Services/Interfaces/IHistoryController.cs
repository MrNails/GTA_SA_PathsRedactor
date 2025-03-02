using System.Windows.Input;

namespace GTA_SA_PathsRedactor.Services.Interfaces
{
    public interface IHistoryController
    {
        IStorableValue? CurrentElement { get; }
        int CurrentPosition { get; }
        bool HasChagned { get; }
        int HistoryCount { get; }
        bool IsOverThreshold { get; }
        bool IsPositionOnEnd { get; }
        bool IsPositionOnStart { get; }
        ICommand UndoCommand { get; }
        ICommand RedoCommand { get; }

        void AddNew(IStorableValue elem);
        void ClearHistory();
        bool Undo();
        bool Redo();
        bool RemoveLast();
        void SetNewOverloadThresholdElem(int index);
    }
}