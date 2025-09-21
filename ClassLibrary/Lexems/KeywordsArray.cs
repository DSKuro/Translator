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
            _keywords = new Keyword[11];
            AddDefaultKeywords();
        }

        private void AddDefaultKeywords()
        {
            AddKeyword(new Keyword("Begin", Lexem.Begin));
            AddKeyword(new Keyword("End", Lexem.End));
            AddKeyword(new Keyword("If", Lexem.If));
            AddKeyword(new Keyword("Then", Lexem.Then));
            AddKeyword(new Keyword("For", Lexem.For));
            AddKeyword(new Keyword("To", Lexem.To));
            AddKeyword(new Keyword("Print", Lexem.Print));
            AddKeyword(new Keyword("Var", Lexem.Var));
            AddKeyword(new Keyword("Const", Lexem.Const));
            AddKeyword(new Keyword("Integer", Lexem.Type));
            AddKeyword(new Keyword("Logical", Lexem.Type));
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
