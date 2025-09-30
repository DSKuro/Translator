namespace ClassLibrary.Files
{
    public class Reader : IDisposable
    {
        private readonly string _filePath;

        private const char _endOfFile = '\0';
        private const char _transfer = '\n';
        private const char _carriage = '\r';
        private const char _tab = '\t';

        private readonly StringReader _streamReader;

        public int SymbolPosition
        {
            get; private set;
        }

        public int NumberOfRow
        {
            get; private set;
        }

        public char CurrentSymbol
        {
            get; private set;
        }

        public Reader(string filePath)
        {
            _filePath = filePath;

            _streamReader = new StringReader(_filePath);
            NumberOfRow = 1;
            SymbolPosition = 0;
            CurrentSymbol = '\0';
            ReadNextSymbol();
        }

        public static string ReadAllFile(string filePath)
        {
            return File.ReadAllText(filePath);
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
                    NumberOfRow++;
                    SymbolPosition = 0;
                    break;

                case _carriage:
                case _tab:
                    ReadNextSymbol();
                    break;

                default:
                    SymbolPosition++;
                    break;
            }
        }

        public void Dispose() 
        {
            _streamReader.Dispose();
        }
    }
}
