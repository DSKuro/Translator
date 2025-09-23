using ClassLibrary.Files;
using ClassLibrary.Generator;
using ClassLibrary.Lexems;
using ClassLibrary.Lexems.Models;
using System.Linq.Expressions;

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
        private string _currentLabel = "";
        private string _forVar = "";
        private string _assignVar = "";
        private bool _hasError = false;
        private bool _awaitBool = false;

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
                if (_analyzer.CurrentLexem != Lexem.Semicolon && _analyzer.CurrentLexem != Lexem.Separator)
                {
                    _analyzer.ProcessNextLexem();
                }
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

            if (_analyzer.CurrentLexem != Lexem.Separator)
            {
                _analyzer.ProcessNextLexem();
            }
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
                    _analyzer.ProcessNextLexem();
                }
            }
            else if (_analyzer.CurrentLexem == Lexem.If)
            {
                ProcessIf();
            }
            else if (_analyzer.CurrentLexem == Lexem.While)
            {
                ProcessWhile();
            }
            else if (_analyzer.CurrentLexem == Lexem.For)
            {
                ProcessFor();
            }
            else if (_analyzer.CurrentLexem == Lexem.Do)
            {
                ProcessDoWhile();
            }
        }

        private void ProcessAssign(Lexem lexem = Lexem.None)
        {
            _analyzer.ProcessNextLexem();
            if (_analyzer.CurrentLexem == Lexem.Assign)
            {
                _forVar = _analyzer.CurrentName;
                Identificator id = _nameTable.GetIdentificator(_forVar);
                if (id != null)
                {
                    if (id.Type == tType.Logical)
                    {
                        _awaitBool = true;
                    }
                }
                    _analyzer.ProcessNextLexem();
                if (lexem == Lexem.To)
                {
                    tType t = ProcessSubExpression();
                    if (t != tType.Integer)
                    {
                        AddError("Нельзя назначить в цикл For нечисловое значение");
                    }
                    CheckLexem(Lexem.To);

                    return;
                }
                tType type = ProcessExpression();
                Identificator x = _nameTable.GetIdentificator(_forVar);
                if (x != null)
                {
                    if (x.Type == tType.Logical && type != tType.Logical)
                    {
                        AddError($"Логической переменной '{_forVar}' можно присваивать только логические выражения (результаты сравнений), получен тип: {type}");
                        _hasError = true;
                    }
                    else if (x.Type == tType.Integer && type != tType.Integer)
                    {
                        AddError($"Целочисленной переменной '{_forVar}' можно присваивать только целочисленные выражения, получен тип: {type}");
                        _hasError = true;
                    }
                }


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
            _awaitBool = false;
        }

        private tType ProcessExpression()
        {
            return ProcessLogicalOr();
        }

        private void ProcessIf()
        {
            CheckLexem(Lexem.If);
            _generator.AddLabel();
            string lowLabel = _generator.GetCurrentLabel();
            _currentLabel = lowLabel;
            _generator.AddLabel();
            string exitLabel = _generator.GetCurrentLabel();
            ProcessExpression();
            CheckLexem(Lexem.Then);
            CheckLexem(Lexem.Separator);
            CheckLexem(Lexem.IfBegin);
            CheckLexem(Lexem.Separator);
            ProcessSequenceInstructions();
            _generator.AddInstruction("jmp " + exitLabel);
            _generator.AddInstruction(lowLabel + ":");
            while (_analyzer.CurrentLexem == Lexem.ElseIf)
            {
                //_generator.AddInstruction(lowLabel + ":");
                _generator.AddLabel();
                lowLabel = _generator.GetCurrentLabel();
                _currentLabel = lowLabel;
                _analyzer.ProcessNextLexem();
                ProcessExpression();
                CheckLexem(Lexem.Then);
                CheckLexem(Lexem.Separator);
                ProcessSequenceInstructions();
                _generator.AddInstruction("jmp " + exitLabel);
                _generator.AddInstruction(lowLabel + ":");
            }
            if (_analyzer.CurrentLexem == Lexem.Else)
            {
                _analyzer.ProcessNextLexem();
                ProcessSequenceInstructions();
            }
            CheckLexem(Lexem.IfEnd);
            CheckLexem(Lexem.Separator);
            _generator.AddInstruction(exitLabel + ":");
        }

        private void ProcessWhile()
        {
            CheckLexem(Lexem.While);
            _generator.AddLabel();
            string upLabel = _generator.GetCurrentLabel();
            _generator.AddLabel();
            string lowLabel = _generator.GetCurrentLabel();
            _currentLabel = lowLabel;
            _generator.AddInstruction(upLabel + ":");
            ProcessExpression();
            CheckLexem(Lexem.Do);
            CheckLexem(Lexem.Separator);
            CheckLexem(Lexem.WhileBegin);
            ProcessSequenceInstructions();
            CheckLexem(Lexem.WhileEnd);
            CheckLexem(Lexem.Separator);
            _generator.AddInstruction("jmp " + upLabel);
            _generator.AddInstruction(lowLabel + ":");
        }

        private void ProcessFor()
        {
            CheckLexem(Lexem.For);
            string varName = _analyzer.CurrentName;
            string startLabel = "";
            string exitLabel = "";
            Identificator x = _nameTable.GetIdentificator(varName);
            if (x == null || x.Category != tCat.Var || x.Type != tType.Integer)
            {
                AddError("Идентификатор не определён");
            }

            ProcessAssign(Lexem.To);
            if (x != null && x.Category == tCat.Var)
            {
                _generator.AddInstruction("pop ax");
                _generator.AddInstruction("mov " + _forVar + ", ax");

                _generator.AddLabel();
                startLabel = _generator.GetCurrentLabel();
                _generator.AddLabel();
                exitLabel = _generator.GetCurrentLabel();

                _generator.AddInstruction(startLabel + ":");

                _generator.AddInstruction("mov ax, " + x.Name);
                _generator.AddInstruction("push ax");
            }

            tType t = ProcessSubExpression();
            if (t != tType.Integer)
            {
                AddError("Нельзя назначить в цикл For нечисловое значение");
            } 

            if (x != null && x.Category == tCat.Var && t == tType.Integer)
            {
                _generator.AddInstruction("pop bx");
                _generator.AddInstruction("pop ax");
                _generator.AddInstruction("cmp ax, bx");
                _generator.AddInstruction("jg " + exitLabel);
            }
            CheckLexem(Lexem.Do);
            CheckLexem(Lexem.Separator);
            CheckLexem(Lexem.ForBegin);
            CheckLexem(Lexem.Separator);

            ProcessSequenceInstructions();

            if (x != null && x.Category == tCat.Var && t == tType.Integer)
            {
                _generator.AddInstruction("mov ax, " + x.Name);
                _generator.AddInstruction("add ax, 1");
                _generator.AddInstruction("mov " + x.Name + ", ax");
                _generator.AddInstruction("jmp " + startLabel);

                _generator.AddInstruction(exitLabel + ":");
            }

            CheckLexem(Lexem.ForEnd);
            CheckLexem(Lexem.Separator);
        }

        private tType ProcessLogicalOr()
        {
            tType leftType = ProcessLogicalAnd();
            while (_analyzer.CurrentLexem == Lexem.Or)
            {
                _analyzer.ProcessNextLexem();
                tType rightType = ProcessLogicalAnd();

                if (leftType != tType.Logical || rightType != tType.Logical)
                {
                    AddError("Логическая операция | применима только к логическим выражениям");
                    leftType = tType.None;
                }
                else
                {
                    _generator.AddInstruction("pop bx"); // правый операнд
                    _generator.AddInstruction("pop ax"); // левый операнд
                    _generator.AddInstruction("or ax, bx");
                    _generator.AddInstruction("push ax");
                    leftType = tType.Logical;
                }
            }
            return leftType;
        }

        private tType ProcessLogicalAnd()
        {
            tType leftType = ProcessComparison();
            while (_analyzer.CurrentLexem == Lexem.And)
            {
                _analyzer.ProcessNextLexem();
                tType rightType = ProcessComparison();

                if (leftType != tType.Logical || rightType != tType.Logical)
                {
                    AddError("Логическая операция & применима только к логическим выражениям");
                    leftType = tType.None;
                }
                else
                {
                    _generator.AddInstruction("pop bx");
                    _generator.AddInstruction("pop ax");
                    _generator.AddInstruction("and ax, bx");
                    _generator.AddInstruction("push ax");
                    leftType = tType.Logical;
                }
            }
            return leftType;
        }

        private tType ProcessComparison()
        {
            tType t = ProcessSumOrSub();
            while (_analyzer.CurrentLexem == Lexem.RightBracket && _bracketLevel <= 0)
            {
                AddError("Найдена лишняя закрывающая скобка");
                _analyzer.ProcessNextLexem();
            }
            if (_analyzer.CurrentLexem == Lexem.Equal ||
                _analyzer.CurrentLexem == Lexem.Not ||
                _analyzer.CurrentLexem == Lexem.Less ||
                _analyzer.CurrentLexem == Lexem.Greater ||
                _analyzer.CurrentLexem == Lexem.LessEqual ||
                _analyzer.CurrentLexem == Lexem.GreaterEqual)
            {
                string transition = "";
                Lexem leftLexem = _analyzer.CurrentLexem;
                switch (_analyzer.CurrentLexem)
                {
                    case Lexem.Equal:
                        transition = "jne";
                        break;

                    case Lexem.Not:
                        transition = "je";
                        break;

                    case Lexem.Greater:
                        transition = "jle";
                        break;

                    case Lexem.GreaterEqual:
                        transition = "jl";
                        break;

                    case Lexem.Less:
                        transition = "jge";
                        break;

                    case Lexem.LessEqual:
                        transition = "jg";
                        break;
                }
                _analyzer.ProcessNextLexem();
                ProcessSumOrSub();
                _generator.AddInstruction("pop ax");
                _generator.AddInstruction("pop bx");
                _generator.AddInstruction("cmp bx, ax");
                _generator.AddInstruction(transition + " " + _currentLabel);
                _currentLabel = "";
                t = tType.Logical;
            }

            return t;
        }

        private void ProcessDoWhile()
        {
            CheckLexem(Lexem.Do);
            CheckLexem(Lexem.Separator);
            CheckLexem(Lexem.DoBegin);
            _generator.AddLabel();
            string upLabel = _generator.GetCurrentLabel();
            _generator.AddLabel();
            string lowLabel = _generator.GetCurrentLabel();
            _currentLabel = lowLabel;
            _generator.AddInstruction(upLabel + ":");
            CheckLexem(Lexem.Separator);
            ProcessSequenceInstructions();
            CheckLexem(Lexem.DoEnd);
            CheckLexem(Lexem.Separator);
            CheckLexem(Lexem.While);
            ProcessExpression();
            CheckLexem(Lexem.Separator);
            _generator.AddInstruction("jmp " + upLabel);
            _generator.AddInstruction(lowLabel + ":");
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
            else if (_analyzer.CurrentLexem == Lexem.Not)
            {
                Lexem unaryOp = _analyzer.CurrentLexem;
                _analyzer.ProcessNextLexem();

                t = ProcessSubExpression();

                if (unaryOp == Lexem.Not)
                {
                    if (t != tType.Logical)
                    {
                        AddError("Унарная операция ! применима только к логическим выражениям");
                        t = tType.None;
                    }
                    else
                    {
                        _generator.AddInstruction("pop ax");
                        _generator.AddInstruction("xor ax, 1");
                        _generator.AddInstruction("push ax");
                    }
                }
                return t;
            }
            else if (_analyzer.CurrentLexem == Lexem.Number)
            {
                tType temp = tType.Integer;
                if (_awaitBool)
                {
                   if ( !(_analyzer.CurrentName == "0" || _analyzer.CurrentName == "1"))
                    {
                        AddError("Ожидалось булево значение");
                    }
                    temp = tType.Logical;
                }
                _generator.AddInstruction("mov ax, " + _analyzer.CurrentName);
                _generator.AddInstruction("push ax");
                _analyzer.ProcessNextLexem();

                //if (_analyzer.CurrentLexem == Lexem.Number || _analyzer.CurrentLexem == Lexem.Name)
                //{
                //    AddError("Ожидался оператор");
                //    _analyzer.ProcessNextLexem();
                //    return tType.None;
                //}
                return temp;
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
                //_analyzer.ProcessNextLexem();
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
                    _analyzer.ProcessNextLexem();
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
                LineNumber = _reader.NumberOfRow,
                Position = _reader.SymbolPosition,
                CurrentSymbol = _reader.CurrentSymbol
                //LineNumber = _analyzer.CurrentRow,
                //Position = _analyzer.CurrentPosition,
                //CurrentSymbol = _analyzer.CurrentSymbol,
            };

            _errors.Add(error);
        }

        public string GetCommands()
        {
            return _generator.GetAllCommands();
        }

        public string ErrorsToString()
        {
            string lexicalErrors = _analyzer.ErrorsToString();
            if (_errors.Count() > 0)
            {
                foreach (SyntaxError error in _errors)
                {
                    lexicalErrors += error.ToString() + "\n";
                }
            }
            return lexicalErrors;
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
