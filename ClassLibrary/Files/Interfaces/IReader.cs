namespace ClassLibrary.Files.Interfaces
{
    public interface IReader : IDisposable
    {
        string ReadAllFile();
        char ReadNextSymbol();
    }
}
