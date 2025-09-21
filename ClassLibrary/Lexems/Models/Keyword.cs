namespace ClassLibrary.Lexems.Models
{
    public class Keyword
    {
        public string Word
        {
            get; 
            set;
        }

        public Lexem Lexem
        {
            get;
            set;
        }

        public Keyword(string word, Lexem lexem)
        {
            Word = word;
            Lexem = lexem;
        }
    }
}
