namespace ClassLibrary.Files.Interfaces
{
    public interface IWriterFactory
    {
        public IWriter CreateWriter(bool append, string path);
    }
}
