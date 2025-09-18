namespace ClassLibrary.Files.Interfaces
{
    public interface IReaderFactory
    {
        public IReader CreateReader(string path);
    }
}
