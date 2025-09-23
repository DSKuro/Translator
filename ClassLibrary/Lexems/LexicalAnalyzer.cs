using ClassLibrary.Files;
using ClassLibrary.Lexems.Exceptions;
using ClassLibrary.Lexems.Models;
using ClassLibrary.Syntax;
using System.Net.Http.Headers;

namespace ClassLibrary.Lexems
{
    public class LexicalAnalyzer
    {
        private readonly Reader _reader;
        private readonly KeywordsArray _keywords;
        private List<LexicalError> _errors = new List<LexicalError>();

        private readonly int MAX_ID_LENGTH = 255;
        public int CurrentRow
        {
            get; private set;
        }

        public int CurrentPosition
        {
            get; private set;
        }

        public char CurrentSymbol
        {
            get; private set;
        }

        public string CurrentName
        {
            get; private set;
        }

        public Lexem CurrentLexem
        {
            get; private set;
        }

        public LexicalAnalyzer(Reader reader)
        {
            _reader = reader;
            _keywords = new KeywordsArray();
            CurrentLexem = Lexem.None;
            CurrentName = "";
        }

        public void ProcessNextLexem()
        {
            CurrentSymbol = _reader.CurrentSymbol;
            CurrentRow = _reader.NumberOfRow;
            CurrentPosition = _reader.SymbolPosition;

            while (_reader.CurrentSymbol == ' ')
            {
                _reader.ReadNextSymbol();
            }

            switch (_reader.CurrentSymbol)
            {
                case '\0':
                    _reader.ReadNextSymbol();
                    CurrentLexem = Lexem.EOF;
                    return;

                case var _ when char.IsLetter(_reader.CurrentSymbol):
                    ProcessIdentificator();
                    return;

                case var _ when char.IsDigit(_reader.CurrentSymbol):
                    ProcessNumber();
                    return;

                case ',':
                    _reader.ReadNextSymbol();
                    CurrentLexem = Lexem.Comma;
                    return;

                case ';':
                    _reader.ReadNextSymbol();
                    CurrentLexem = Lexem.Semicolon;
                    return;

                case '\n':
                    _reader.ReadNextSymbol();
                    CurrentLexem = Lexem.Separator;
                    return;

                case '<':
                    _reader.ReadNextSymbol();
                    if (_reader.CurrentSymbol == '=')
                    {
                        _reader.ReadNextSymbol();
                        CurrentLexem = Lexem.LessEqual;
                    }
                    else
                    {
                        CurrentLexem = Lexem.Less;
                    }
                    return;

                case '>':
                    _reader.ReadNextSymbol();
                    CurrentLexem = _reader.CurrentSymbol == '=' ? Lexem.GreaterEqual : Lexem.Greater;
                    return;

                case '+': _reader.ReadNextSymbol(); CurrentLexem = Lexem.Plus; return;
                case '-': _reader.ReadNextSymbol(); CurrentLexem = Lexem.Minus; return;
                case '*': _reader.ReadNextSymbol(); CurrentLexem = Lexem.Multiplication; return;
                case '/': _reader.ReadNextSymbol(); CurrentLexem = Lexem.Division; return;
                case '=': _reader.ReadNextSymbol(); CurrentLexem = Lexem.Equal; return;

                case ':':
                    _reader.ReadNextSymbol();
                    if (_reader.CurrentSymbol == '=')
                    {
                        _reader.ReadNextSymbol();
                        CurrentLexem = Lexem.Assign;
                    }
                    else
                    {
                        CurrentLexem = Lexem.Colon;
                    }
                    return;

                case '(': _reader.ReadNextSymbol(); CurrentLexem = Lexem.LeftBracket; return;
                case ')': _reader.ReadNextSymbol(); CurrentLexem = Lexem.RightBracket; return;
                case '!': 
                    _reader.ReadNextSymbol();
                    CurrentLexem = _reader.CurrentSymbol == '=' ? Lexem.NotEqual : Lexem.Not ; return;
                case '&': _reader.ReadNextSymbol(); CurrentLexem = Lexem.And; return;
                case '|': _reader.ReadNextSymbol(); CurrentLexem = Lexem.Or; return;
                case '^': _reader.ReadNextSymbol(); CurrentLexem = Lexem.XOR; return;

                default:
                    AddError("Лексема не распознана");
                    break;
            }
        }

        private void ProcessIdentificator()
        {
            ProcessSymbols(char.IsLetter);
            if (CurrentName.Length >= MAX_ID_LENGTH)
            {
                ProcessError("Лексическая ошибка: идентификатор не может быть длиннее 255");
            }
            CurrentLexem = _keywords.GetKeywordLexem(CurrentName);
        }

        private void ProcessNumber()
        {
            ProcessSymbols(char.IsDigit);
            if (int.TryParse(CurrentName, out int numberValue))
            {
                if (numberValue > int.MaxValue)
                {
                    ProcessError($"Числовое значение превышает максимально допустимое ({int.MaxValue})");
                    return;
                }
            }
            else
            {
                ProcessError("Недопустимое числовое значение");
            }
            CurrentLexem = Lexem.Number;
        }

        private void ProcessSymbols(Predicate<char> predicate)
        {
            string lexem = "";
            do
            {
                
                lexem += _reader.CurrentSymbol;
                _reader.ReadNextSymbol();
            }
            while (predicate.Invoke(_reader.CurrentSymbol));
            CurrentName = lexem;
        }

        private void ProcessError(string message)
        {
            CurrentName = "";
            CurrentLexem = Lexem.None;
            throw new LexicalException(message);
        }


        private void AddError(string message)
        {
            LexicalError error = new LexicalError
            {
                Message = message,
                LineNumber = CurrentRow,
                Position = CurrentPosition,
                CurrentSymbol = CurrentSymbol,
            };

            _errors.Add(error);
        }

        public string ErrorsToString()
        {
            if (_errors.Count() > 0)
            {
                string temp = "";
                foreach (LexicalError error in _errors)
                {
                    temp += error.ToString() + "\n";
                }
                return temp;
            }
            return "";

        }
    }

    public class LexicalError
    {
        public string Message { get; set; }
        public int LineNumber { get; set; }
        public int Position { get; set; }
        public char CurrentSymbol { get; set; }

        public override string ToString()
        {
            return $"Лексическая ошибка в строке {LineNumber}, позиция {Position}: {Message} (текущий символ: '{CurrentSymbol}')";
        }
    }
}
