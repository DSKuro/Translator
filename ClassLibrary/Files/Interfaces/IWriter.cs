namespace ClassLibrary.Files.Interfaces
{
    public interface IWriter : IDisposable
    {
        public void WriteToFile(string data);
    }
}
