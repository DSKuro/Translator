namespace ClassLibrary.Files
{
    public class Reader : IDisposable
    {
        private readonly string _filePath;

        private const char _endOfFile = '\0';
        private const char _transfer = '\n';
        private const char _carriage = '\r';
        private const char _tab = '\t';

        private int _numberOfRow;
        private int _symbolPosition;

        private readonly StreamReader _streamReader;

        public char CurrentSymbol
        {
            get; private set;
        }

        public Reader(string filePath)
        {
            _filePath = filePath;

            if (File.Exists(_filePath))
            {
                _streamReader = new StreamReader(_filePath);
                _numberOfRow = 1;
                _symbolPosition = 0;
                CurrentSymbol = '\0';
                ReadNextSymbol();
            }
        }

        public string ReadAllFile()
        {
            return File.ReadAllText(_filePath);
        }

        public void ReadNextSymbol()
        {
            int nextSymbol = _streamReader.Read();
            if (nextSymbol == -1)
            {
                CurrentSymbol = _endOfFile;
                return;
            }
            CurrentSymbol = Convert.ToChar(nextSymbol);
            ProcessSymbol();
        }

        private void ProcessSymbol()
        {
            switch (CurrentSymbol)
            {
                case _transfer:
                    _numberOfRow++;
                    _symbolPosition = 0;
                    break;

                case _carriage:
                case _tab:
                    ReadNextSymbol();
                    break;

                default:
                    _symbolPosition++;
                    break;
            }
        }

        public void Dispose() 
        {
            _streamReader.Dispose();
        }
    }
}
