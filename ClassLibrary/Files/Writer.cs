using ClassLibrary.Files.Interfaces;

namespace ClassLibrary.Files
{
    public class Writer : IWriter
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
