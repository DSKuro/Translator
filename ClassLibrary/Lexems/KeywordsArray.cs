using ClassLibrary.Lexems.Models;

namespace ClassLibrary.Lexems
{
    public class KeywordsArray
    {
        private int _keywordPointer;
        private readonly Keyword[] _keywords;

        public KeywordsArray()
        {
            _keywordPointer = 0;
            _keywords = new Keyword[23];
            AddDefaultKeywords();
        }

        private void AddDefaultKeywords()
        {
            AddKeyword(new Keyword("Begin", Lexem.Begin));
            AddKeyword(new Keyword("End", Lexem.End));
            AddKeyword(new Keyword("If", Lexem.If));
            AddKeyword(new Keyword("Then", Lexem.Then));
            AddKeyword(new Keyword("IfBegin", Lexem.IfBegin));
            AddKeyword(new Keyword("IfEnd", Lexem.IfEnd));
            AddKeyword(new Keyword("ElseIf", Lexem.ElseIf));
            AddKeyword(new Keyword("Else", Lexem.Else));
            AddKeyword(new Keyword("While", Lexem.While));
            AddKeyword(new Keyword("WhileBegin", Lexem.WhileBegin));
            AddKeyword(new Keyword("WhileEnd", Lexem.WhileEnd));
            AddKeyword(new Keyword("Do", Lexem.Do));
            AddKeyword(new Keyword("DoBegin", Lexem.DoBegin));
            AddKeyword(new Keyword("DoEnd", Lexem.DoEnd));
            AddKeyword(new Keyword("For", Lexem.For));
            AddKeyword(new Keyword("To", Lexem.To));
            AddKeyword(new Keyword("ForBegin", Lexem.ForBegin));
            AddKeyword(new Keyword("ForEnd", Lexem.ForEnd));
            AddKeyword(new Keyword("Print", Lexem.Print));
            AddKeyword(new Keyword("Var", Lexem.Var));
            AddKeyword(new Keyword("Const", Lexem.Const));
            AddKeyword(new Keyword("Integer", Lexem.Integer));
            AddKeyword(new Keyword("Logical", Lexem.Logical));
        }

        public void AddKeyword(Keyword keyword)
        {
            _keywords[_keywordPointer++] = keyword;
        }

        public Lexem GetKeywordLexem(string word)
        {
            for (int i = _keywordPointer - 1; i >= 0; i--)
            {
                if (_keywords[i].Word == word)
                {
                    return _keywords[i].Lexem;
                }
            }
            return Lexem.Name;
        }
    }
}
