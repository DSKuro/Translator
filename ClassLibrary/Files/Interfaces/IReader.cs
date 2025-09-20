namespace ClassLibrary.Files.Interfaces
{
    public interface IReader : IDisposable
    {
        public string ReadAllFile();
        public char ReadNextSymbol();
    }
}
