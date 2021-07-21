namespace GTA_SA_PathsRedactor.Services
{
    public enum State : byte
    {
        Added,
        Moved,
        Deleted
    }

    public interface IStorableValue
    {
        object Value { get; }

        State State { get; }
    }
}
