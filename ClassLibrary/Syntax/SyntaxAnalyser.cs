using ClassLibrary.Files;
using ClassLibrary.Generator;
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
        private readonly CodeGenerator _generator;

        public List<SyntaxError> Errors => _errors;

        public SyntaxAnalyser(Reader reader)
        {
            _analyzer = new LexicalAnalyzer(reader);
            _nameTable = new NameTable();
            _errors = new List<SyntaxError>();
            _reader = reader;
            _generator = new CodeGenerator(_nameTable);
            _analyzer.ProcessNextLexem();
        }

        public bool Compile()
        {
            _generator.DeclareDataSegment();
            ProcessVariablesDeclaration();
            _generator.DeclareVariables();
            _generator.DeclareStackCodeSegment();
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
            _generator.DeclareMainProcedureEnd();
            _generator.DeclareCodeEnd();
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

            if (_analyzer.CurrentLexem != Lexem.Integer && _analyzer.CurrentLexem != Lexem.Logical)
            {
                AddError($"Ожидался тип данных после двоеточия, но получена лексема '{_analyzer.CurrentLexem}'");
            }
            else
            {
                if (_analyzer.CurrentLexem == Lexem.Integer)
                {
                    _nameTable.SetTypeForAllVariables(tType.Integer);
                }
                else
                {
                    _nameTable.SetTypeForAllVariables(tType.Logical);
                }
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
                    CheckLexem(Lexem.Semicolon);
                }
                else
                {
                    AddError($"Идентификатор {_analyzer.CurrentName} не определён");
                    _analyzer.ProcessNextLexem();
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
            //while (_analyzer.CurrentLexem != Lexem.Separator)
            //{
            //    _analyzer.ProcessNextLexem();
            //}
            //return tType.Integer;
            return ProcessSumOrSub();
        }

        private tType ProcessSumOrSub()
        {
            tType t;
            Lexem op;
            if (_analyzer.CurrentLexem == Lexem.Plus ||
                _analyzer.CurrentLexem == Lexem.Minus)
            {
                op = _analyzer.CurrentLexem;
                _analyzer.ProcessNextLexem();
                t = ProcessMultOrDiv();
            }
            else
            {
                t = ProcessMultOrDiv();
            }
            if (_analyzer.CurrentLexem == Lexem.Plus ||
                _analyzer.CurrentLexem == Lexem.Minus)
            {
                do
                {
                    op = _analyzer.CurrentLexem;
                    _analyzer.ProcessNextLexem();

                    if (t != tType.Integer)
                    {
                        AddError("Арифметические операции + и - применимы только к целым числам");
                        t = tType.None;
                    }

                    t = ProcessMultOrDiv();

                    if (t != tType.Integer)
                    {
                        AddError("Арифметические операции + и - применимы только к целым числам");
                        t = tType.None;
                    }
                   
                    switch (op)
                    {
                        case Lexem.Plus:
                        case Lexem.Minus:
                            break;
                    }
                } while (_analyzer.CurrentLexem == Lexem.Plus ||
                _analyzer.CurrentLexem == Lexem.Minus);
            }
            return t;
        }

        private tType ProcessMultOrDiv()
        {
            Lexem op;
            tType t = ProcessSubExpression();
            if (_analyzer.CurrentLexem == Lexem.Multiplication ||
                _analyzer.CurrentLexem == Lexem.Division)
            {
                do
                {
                    op = _analyzer.CurrentLexem;
                    _analyzer.ProcessNextLexem();

                    if (t != tType.Integer)
                    {
                        AddError("Арифметические операции * и / применимы только к целым числам");
                        t = tType.None;
                    }

                    t = ProcessSubExpression();

                    if (t != tType.Integer)
                    {
                        AddError("Арифметические операции + и - применимы только к целым числам");
                        t = tType.None;
                    }

                    switch (op)
                    {
                        case Lexem.Multiplication:
                        case Lexem.Division:
                            break;
                    }
                } while (_analyzer.CurrentLexem == Lexem.Multiplication ||
                _analyzer.CurrentLexem == Lexem.Division);
            }
            return t;
        }

        private tType ProcessSubExpression()
        {
            Identificator x;
            tType t = tType.None;
            if (_analyzer.CurrentLexem == Lexem.Name)
            {
                x = _nameTable.GetIdentificator(_analyzer.CurrentName);
                if (x != null && x.Category == tCat.Var)
                {
                    _analyzer.ProcessNextLexem();
                    return x.Type;
                }
                else
                {
                    AddError($"Не удалось определить идентификатор '{_analyzer.CurrentName}'");
                    return tType.None;
                }
            }
            else if (_analyzer.CurrentLexem == Lexem.Number)
            {
                _analyzer.ProcessNextLexem();
                return tType.Integer;
            }
            else if (_analyzer.CurrentLexem == Lexem.LeftBracket)
            {
                _analyzer.ProcessNextLexem();
                t = ProcessExpression();
                CheckLexem(Lexem.RightBracket);
                return t;
            }
            else
            {
                AddError("Не удалось разобрать выражение");
                return t;
            }
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

        public string GetCommands()
        {
            return _generator.GetAllCommands();
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
