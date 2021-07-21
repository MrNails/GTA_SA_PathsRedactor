namespace GTA_SA_PathsRedactor.Services
{
    public interface ITrackable
    {
        bool HasChanged();

        void AcceptChanged();
        void ClearChanged();
    }
}
