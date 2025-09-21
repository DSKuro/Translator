using ClassLibrary.Files;
using ClassLibrary.Lexems;
using ClassLibrary.Lexems.Models;

namespace ClassLibrary.Syntax
{
    public class SyntaxAnalyser
    {
        private readonly LexicalAnalyzer _analyzer;
        private readonly NameTable _nameTable;
        private readonly List<SyntaxError> _errors;
        private readonly Reader _reader;

        public List<SyntaxError> Errors => _errors;

        public SyntaxAnalyser(Reader reader)
        {
            _analyzer = new LexicalAnalyzer(reader);
            _nameTable = new NameTable();
            _errors = new List<SyntaxError>();
            _reader = reader;
            _analyzer.ProcessNextLexem();
        }

        public bool Compile()
        {
            ProcessVariablesDeclaration();
            CheckLexem(Lexem.Separator);
            if (_analyzer.CurrentLexem != Lexem.Begin)
            {
                AddError($"Ожидалась лексема 'Begin', но получена '{_analyzer.CurrentLexem}'");
            }
            else
            {
                _analyzer.ProcessNextLexem();
            }

            _analyzer.ProcessNextLexem();
            ProcessSequenceInstructions();
            CheckLexem(Lexem.End);
            return _errors.Count == 0;
        }

        private void CheckLexem(Lexem awaitedLexem)
        {
            if (_analyzer.CurrentLexem != awaitedLexem)
            {
                AddError($"Ожидалась лексема '{awaitedLexem}', но получена '{_analyzer.CurrentLexem}'");
            }

            _analyzer.ProcessNextLexem();
        }

        private void ProcessVariablesDeclaration()
        {
            CheckLexem(Lexem.Var);
            if (_analyzer.CurrentLexem != Lexem.Name)
            {
                AddError($"Ожидался идентификатор, но получена лексема '{_analyzer.CurrentLexem}'");
                _analyzer.ProcessNextLexem();
            }
            else
            {
                _nameTable.AddIdentificator(_analyzer.CurrentName, tCat.Var);
                _analyzer.ProcessNextLexem();
            }

            while (_analyzer.CurrentLexem == Lexem.Comma)
            {
                _analyzer.ProcessNextLexem();
                if (_analyzer.CurrentLexem != Lexem.Name)
                {
                    AddError($"Ожидался идентификатор после запятой, но получена лексема '{_analyzer.CurrentLexem}'");
                    _analyzer.ProcessNextLexem();
                }
                else
                {
                    _nameTable.AddIdentificator(_analyzer.CurrentName, tCat.Var);
                    _analyzer.ProcessNextLexem();
                }
            }

            if (_analyzer.CurrentLexem != Lexem.Colon)
            {
                AddError($"Ожидалось двоеточие после идентификаторов, но получена лексема '{_analyzer.CurrentLexem}'");
            }

            _analyzer.ProcessNextLexem();

            if (_analyzer.CurrentLexem != Lexem.Type)
            {
                AddError($"Ожидался тип данных после двоеточия, но получена лексема '{_analyzer.CurrentLexem}'");
            }

            _analyzer.ProcessNextLexem();

            if (_analyzer.CurrentLexem != Lexem.Semicolon)
            {
                AddError($"Ожидалась точка с запятой после типа данных, но получена лексема '{_analyzer.CurrentLexem}'");
            }

            _analyzer.ProcessNextLexem();
        }

        private void ProcessSequenceInstructions()
        {
            ProcessInstruction();
            while (_analyzer.CurrentLexem == Lexem.Separator)
            {
                _analyzer.ProcessNextLexem();
                ProcessInstruction();
            }
        }

        private void ProcessInstruction()
        {
            if (_analyzer.CurrentLexem == Lexem.Name)
            {
                Identificator x = _nameTable.GetIdentificator(_analyzer.CurrentName);
                if (x != null)
                {
                    ProcessAssign();
                }
                else
                {
                    AddError($"Идентификатор {_analyzer.CurrentName} не определён");
                }
            }
        }

        private void ProcessAssign()
        {
            _analyzer.ProcessNextLexem();
            if (_analyzer.CurrentLexem == Lexem.Assign)
            {
                _analyzer.ProcessNextLexem();
                ProcessExpression();
            }
            else
            {
                AddError($"Ожидалось присваивание, но получена лексема {_analyzer.CurrentLexem}");
            }
        }

        private tType ProcessExpression()
        {
            while (_analyzer.CurrentLexem != Lexem.Separator)
            {
                _analyzer.ProcessNextLexem();
            }
            return tType.Integer;
        }

        private void AddError(string message)
        {
            SyntaxError error = new SyntaxError
            {
                Message = message,
                //LineNumber = _reader.NumberOfRow,
                //Position = _reader.SymbolPosition,
                //CurrentSymbol = _reader.CurrentSymbol
                LineNumber = _analyzer.CurrentRow,
                Position = _analyzer.CurrentPosition,
                CurrentSymbol = _analyzer.CurrentSymbol,
            };

            _errors.Add(error);
        }
    }

    public class SyntaxError
    {
        public string Message { get; set; }
        public int LineNumber { get; set; }
        public int Position { get; set; }
        public char CurrentSymbol { get; set; }

        public override string ToString()
        {
            return $"Синтаксическая ошибка в строке {LineNumber}, позиция {Position}: {Message} (текущий символ: '{CurrentSymbol}')";
        }
    }
}
