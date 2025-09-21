namespace ClassLibrary.Lexems
{
    public enum tCat
    {
        Const, Var, Type
    }

    public enum tType
    {
        None, Integer, Logical
    }

    public class Identificator
    {
        public string Name
        {
            get; set;
        }

        public tCat Category
        {
            get; set;
        }

        public tType Type
        {
            get; set;
        }
    }
}
