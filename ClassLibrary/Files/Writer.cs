namespace ClassLibrary.Files
{
    public class Writer : IDisposable
    {
        private readonly StreamWriter _writer;

        public Writer(bool append, string path) 
        {
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            _writer = new StreamWriter(path, append);
        }

        public void WriteToFile(string data)
        {
            _writer.Write(data);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
