using ClassLibrary.Lexems;

namespace ClassLibrary.Generator
{
    public class CodeGenerator
    {
        private static readonly int MAX_NUMBER_STRINGS = 255;

        private string[] _code = new string[MAX_NUMBER_STRINGS];
        private int _codePointer = 0;

        private readonly NameTable _nameTable;

        public CodeGenerator(NameTable nameTable)
        {
            _nameTable = nameTable;
        }

        public void AddInstruction(string instruction)
        {
            _code[_codePointer++] = instruction;
        }

        public void DeclareDataSegment()
        {
            AddInstruction("data segment para public \"data\"");
        }

        public void DeclareStackCodeSegment()
        {
            AddInstruction("PRINT_BUF DB ' ' DUP(10)");
            AddInstruction("BUFEND    DB '$'");
            AddInstruction("data ends");
            AddInstruction("stk segment stack");
            AddInstruction("db 256 dup (\"?\")");
            AddInstruction("stk ends");
            AddInstruction("code segment para public \"code\"");
            AddInstruction("main proc");
            AddInstruction("assume cs:code,ds:data,ss:stk");
            AddInstruction("mov ax,data");
            AddInstruction("mov ds,ax");
        }

        public void DeclareMainProcedureEnd()
        {
            AddInstruction("mov ax,4c00h");
            AddInstruction("int 21h");
            AddInstruction("main endp");
        }

        public void DeclareCodeEnd()
        {
            AddInstruction("code ends");
            AddInstruction("end main");
        }

        public void DeclareVariables()
        {
            LinkedListNode<Identificator> node = _nameTable.GetAllIdentificators().First;
            while (node != null)
            {
                AddInstruction(node.Value.Name + " dw 1");
                node = node.Next;
            }
        }

        public string GetAllCommands()
        {
            string[] commands = new string[_codePointer + 1];
            Array.Copy(_code, commands, _codePointer + 1);

            return string.Join(Environment.NewLine, commands);
        }
    }
}
