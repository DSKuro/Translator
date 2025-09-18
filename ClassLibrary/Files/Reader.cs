using ClassLibrary.Files.Interfaces;

namespace ClassLibrary.Files
{
    public class Reader : IReader, IDisposable
    {
        private readonly string _filePath;

        private const int _endOfFile = -1;
        private const int _transfer = '\n';
        private const int _carriage = '\r';
        private const int _tab = '\t';

        private int _numberOfRow;
        private int _symbolPosition;
        private char _currentSymbol;

        private readonly StreamReader _streamReader;

        public Reader(string filePath)
        {
            _filePath = filePath;

            if (File.Exists(_filePath))
            {
                _streamReader = new StreamReader(_filePath);
                _numberOfRow = 1;
                _symbolPosition = 0;
            }
        }

        public string ReadAllFile()
        {
            return File.ReadAllText(_filePath);
        }

        public char ReadNextSymbol()
        {
            int currentSymbol = _streamReader.Read();
            return ProcessSymbol(currentSymbol);
        }

        private char ProcessSymbol(int currentSymbol)
        {
            switch (currentSymbol)
            {
                case _endOfFile:
                    _currentSymbol = Convert.ToChar(_endOfFile);
                    break;

                case _transfer:
                    _numberOfRow++;
                    _symbolPosition = 0;
                    break;

                case _carriage:
                case _tab:
                    return ReadNextSymbol();

                default:
                    _symbolPosition++;
                    _currentSymbol = Convert.ToChar(currentSymbol);
                    break;
            }
            return _currentSymbol;
        }

        public void Dispose() 
        {
            _streamReader.Dispose();
        }
    }
}
