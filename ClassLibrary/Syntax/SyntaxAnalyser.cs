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

        private int _bracketLevel = 0;
        private bool _hasError = false;

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

            ProcessSequenceInstructions();
            _generator.DeclareMainProcedureEnd();
            _generator.DeclarePrint();
            _generator.DeclareCodeEnd();
            return _errors.Count == 0;
            
        }

        private bool CheckLexem(Lexem awaitedLexem)
        {
            if (_analyzer.CurrentLexem != awaitedLexem)
            {
                AddError($"Ожидалась лексема '{awaitedLexem}', но получена '{_analyzer.CurrentLexem}'");
                return false;
            }
            else
            {
                _analyzer.ProcessNextLexem();
                return true;
            }
        }

        private void ProcessVariablesDeclaration()
        {
            CheckLexem(Lexem.Var);
            if (_analyzer.CurrentLexem != Lexem.Name)
            {
                AddError($"Ожидался идентификатор, но получена лексема '{_analyzer.CurrentLexem}'");
            }
            else
            {
                _nameTable.AddIdentificator(_analyzer.CurrentName, tCat.Var);
                _analyzer.ProcessNextLexem();
            }

            while (_analyzer.CurrentLexem == Lexem.Comma || _analyzer.CurrentLexem == Lexem.Name)
            {
                if (_analyzer.CurrentLexem == Lexem.Comma)
                {
                    _analyzer.ProcessNextLexem();
                    if (_analyzer.CurrentLexem != Lexem.Name)
                    {
                        AddError($"Ожидался идентификатор после запятой, но получена лексема '{_analyzer.CurrentLexem}'");
                    }
                    else
                    {
                        _nameTable.AddIdentificator(_analyzer.CurrentName, tCat.Var);
                        _analyzer.ProcessNextLexem();
                    }
                }
                else
                {
                    AddError($"Ожидалась запятая между идентификаторами, но получена лексема {_analyzer.CurrentLexem}");
                    _analyzer.ProcessNextLexem();
                }

            }

            if (_analyzer.CurrentLexem != Lexem.Colon)
            {
                AddError($"Ожидалось двоеточие после идентификаторов, но получена лексема '{_analyzer.CurrentLexem}'");
            }
            else
            {
                _analyzer.ProcessNextLexem();
            }

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
                _analyzer.ProcessNextLexem();
            }

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
            CheckLexem(Lexem.End);
            CheckLexem(Lexem.Separator);
            if (_analyzer.CurrentLexem != Lexem.Print)
            {
                AddError($"Ожидалась инструкция Print, но получена лексема {_analyzer.CurrentLexem}");
            }
            else
            {
                ProcessPrintInstruction();
            }
            if (_analyzer.CurrentLexem != Lexem.EOF)
            {
                AddError($"Ожидался конец файла, но получена лексема {_analyzer.CurrentLexem}");
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
                    if (!_hasError)
                    {
                        _generator.AddInstruction("pop ax");
                        _generator.AddInstruction("mov " + x.Name + ", ax");
                    }
                    _hasError = false;
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
                if (_analyzer.CurrentLexem != Lexem.Separator && _analyzer.CurrentLexem != Lexem.Semicolon)
                {
                    AddError("Отсутствует операнд");
                    ProcessExpression();
                }
            }
            else
            {
                _hasError = true;
                AddError($"Ожидалось присваивание, но получена лексема {_analyzer.CurrentLexem}");
                if (_analyzer.CurrentLexem == Lexem.Separator)
                {
                    AddError("Ожидалось выражение");
                    return;
                }
                _analyzer.ProcessNextLexem();
                ProcessExpression();
            }
        }

        private bool IsOperand(Lexem lexem)
        {
            return lexem == Lexem.Name || lexem == Lexem.Number || lexem == Lexem.LeftBracket;
        }

        private tType ProcessExpression()
        {
            tType t = ProcessSumOrSub();
            while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
            {
                AddError("Найдена лишняя закрывающая скобка");
                _analyzer.ProcessNextLexem();
            }
            return t;
        }

        private tType ProcessSumOrSub()
        {
            tType t;
            Lexem op;
            //if (_analyzer.CurrentLexem == Lexem.Plus ||
            //    _analyzer.CurrentLexem == Lexem.Minus)
            //{
            //    op = _analyzer.CurrentLexem;
            //    _analyzer.ProcessNextLexem();
            //    t = ProcessMultOrDiv();
            //}
            //else
            //{
            t = ProcessMultOrDiv();
            //}
            if (_analyzer.CurrentLexem == Lexem.Plus ||
                _analyzer.CurrentLexem == Lexem.Minus)
            {
                do
                {
                    op = _analyzer.CurrentLexem;
                    _analyzer.ProcessNextLexem();

                    while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
                    {
                        AddError("Найдена лишняя закрывающая скобка");
                        _analyzer.ProcessNextLexem();
                    }

                    if (t != tType.Integer)
                    {
                        string temp = "";
                        if (t == tType.None)
                        {
                            temp = "Левый операнд не является ни числом, ни булевым значением";
                        }
                        else
                        {
                            temp = "Арифметические операции * и / применимы только к целым числам, левый операнд - булево значение";
                        }
                        AddError(temp);
                        t = tType.None;
                    }

                    t = ProcessMultOrDiv();


                    if (t != tType.Integer)
                    {
                        string temp = "";
                        if (t == tType.None)
                        {
                            temp = "Правый операнд не является ни числом, ни булевым значением";
                        }
                        else
                        {
                            temp = "Арифметические операции * и / применимы только к целым числам, правый  операнд - булево значение";
                        }
                        AddError(temp);
                        t = tType.None;
                    }

                    switch (op)
                    {
                        case Lexem.Plus:
                            _generator.AddInstruction("pop bx");
                            _generator.AddInstruction("pop ax");
                            _generator.AddInstruction("add ax, bx");
                            _generator.AddInstruction("push ax");
                            break;
                        case Lexem.Minus:
                            _generator.AddInstruction("pop bx");
                            _generator.AddInstruction("pop ax");
                            _generator.AddInstruction("sub ax, bx");
                            _generator.AddInstruction("push ax");
                            break;
                    }

                    while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
                    {
                        AddError("Найдена лишняя закрывающая скобка");
                        _analyzer.ProcessNextLexem();
                    }

                } while (_analyzer.CurrentLexem == Lexem.Plus ||
                _analyzer.CurrentLexem == Lexem.Minus);
            }
            while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
            {
                AddError("Найдена лишняя закрывающая скобка");
                _analyzer.ProcessNextLexem();
            }
            return t;
        }

        private tType ProcessMultOrDiv()
        {
            Lexem op;
            tType t = ProcessSubExpression();
            while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
            {
                AddError("Найдена лишняя закрывающая скобка");
                _analyzer.ProcessNextLexem();
            }

            if (_analyzer.CurrentLexem == Lexem.Multiplication ||
                _analyzer.CurrentLexem == Lexem.Division)
            {
                do
                {
                    op = _analyzer.CurrentLexem;
                    _analyzer.ProcessNextLexem();

                    while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
                    {
                        AddError("Найдена лишняя закрывающая скобка");
                        _analyzer.ProcessNextLexem();
                    }

                    if (t != tType.Integer)
                    {
                        string temp = "";
                        if (t == tType.None)
                        {
                            temp = "Левый операнд не является ни числом, ни булевым значением";
                        }
                        else
                        {
                            temp = "Арифметические операции * и / применимы только к целым числам, левый операнд - булево значение";
                        }
                        AddError(temp);
                        t = tType.None;
                    }

                    t = ProcessSubExpression();

                    while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
                    {
                        AddError("Найдена лишняя закрывающая скобка");
                        _analyzer.ProcessNextLexem();
                    }

                    if (t != tType.Integer)
                    {
                        string temp = "";
                        if (t == tType.None)
                        {
                            temp = "Правый операнд не является ни числом, ни булевым значением";
                        }
                        else
                        {
                            temp = "Арифметические операции * и / применимы только к целым числам, правый операнд - булево значение";
                        }
                        AddError(temp);
                        t = tType.None;
                    }
                    switch (op)
                    {
                        case Lexem.Multiplication:
                            _generator.AddInstruction("pop bx");
                            _generator.AddInstruction("pop ax");
                            _generator.AddInstruction("mul bx");
                            _generator.AddInstruction("push ax");
                            break;
                        case Lexem.Division:
                            _generator.AddInstruction("pop bx");
                            _generator.AddInstruction("pop ax");
                            _generator.AddInstruction("cwd");
                            _generator.AddInstruction("div bl");
                            _generator.AddInstruction("push ax");
                            break;
                    }
                    while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
                    {
                        AddError("Найдена лишняя закрывающая скобка");
                        _analyzer.ProcessNextLexem();
                    }
                } while (_analyzer.CurrentLexem == Lexem.Multiplication ||
                _analyzer.CurrentLexem == Lexem.Division);
            }
            while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
            {
                AddError("Найдена лишняя закрывающая скобка");
                _analyzer.ProcessNextLexem();
            }
            return t;
        }

        private tType ProcessSubExpression()
        {
            Identificator x;
            tType t = tType.None;
            if (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
            {
                AddError("Найдена закрывающая скобка без соответствующей открывающей");
                _analyzer.ProcessNextLexem();
                return tType.None;
            }
            if (_analyzer.CurrentLexem == Lexem.Name)
            {
                x = _nameTable.GetIdentificator(_analyzer.CurrentName);
                if (x != null && x.Category == tCat.Var)
                {
                    _generator.AddInstruction("mov ax, " + _analyzer.CurrentName);
                    _generator.AddInstruction("push ax");
                    _analyzer.ProcessNextLexem();
                    //if (_analyzer.CurrentLexem == Lexem.Number || _analyzer.CurrentLexem == Lexem.Name)
                    //{
                    //    AddError("Ожидался оператор");
                    //    _analyzer.ProcessNextLexem();
                    //}
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
                _generator.AddInstruction("mov ax, " + _analyzer.CurrentName);
                _generator.AddInstruction("push ax");
                _analyzer.ProcessNextLexem();
                
                //if (_analyzer.CurrentLexem == Lexem.Number || _analyzer.CurrentLexem == Lexem.Name)
                //{
                //    AddError("Ожидался оператор");
                //    _analyzer.ProcessNextLexem();
                //    return tType.None;
                //}
                return tType.Integer;
            }
            else if (_analyzer.CurrentLexem == Lexem.LeftBracket)
            {
                _bracketLevel++;
                _analyzer.ProcessNextLexem();
                t = ProcessExpression();
                if (_analyzer.CurrentLexem == Lexem.RightBracket)
                {
                    _bracketLevel--; // Уменьшаем уровень вложенности
                    _analyzer.ProcessNextLexem();
                }
                else
                {
                    AddError("Ожидалась закрывающая скобка");
                }
                return t;
            }
            else if (_analyzer.CurrentLexem == Lexem.Separator)
            {
                AddError("Ожидалось выражение");
                return t;
            }
            else
            {
                AddError("Текущий символ не является ни числом, ни идентификатором");
                _analyzer.ProcessNextLexem();
                return t;
            }
        }

        private void ProcessPrintInstruction()
        {
            CheckLexem(Lexem.Print);
            if (_analyzer.CurrentLexem == Lexem.Name)
            {
                Identificator x = _nameTable.GetIdentificator(_analyzer.CurrentName);
                if (x != null)
                {
                    _generator.AddInstruction("push ax");
                    _generator.AddInstruction("mov ax, " + _analyzer.CurrentName);
                    _generator.AddInstruction("CALL PRINT");
                    _generator.AddInstruction("pop ax");
                    _analyzer.ProcessNextLexem();
                }
                else
                {
                    AddError($"Идентификатор {_analyzer.CurrentName} не определён");
                }
            }
            else
            {
                AddError($"Ожидался идентификатор, но получен {_analyzer.CurrentLexem}");
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
