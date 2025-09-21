using ClassLibrary.Lexems.Exceptions;

namespace ClassLibrary.Lexems
{
    public class NameTable
    {
        private LinkedList<Identificator> _identificators;

        public NameTable()
        {
            _identificators = new LinkedList<Identificator>();
        }

        public Identificator AddIdentificator(string name, tCat category)
        {
            if (ContainsIdentificator(name))
            {
                throw new IdentificatorException($"Идентификатор с именем '{name}' уже существует");
            }
            Identificator identificator = new Identificator();
            identificator.Name = name;
            identificator.Category = category;
            _identificators.AddLast(identificator);
            return identificator;
        }

        public bool ContainsIdentificator(string name)
        {
            LinkedListNode<Identificator> node = _identificators.First;
            while (node != null)
            {
                if (node.Value.Name == name)
                {
                    return true;
                }
                node = node.Next;
            }
            return false;
        }

        public Identificator GetIdentificator(string name)
        {
            LinkedListNode<Identificator> node = _identificators.First;
            while (node != null && node.Value.Name != name)
            {
                node = node.Next;
            }
            return node?.Value;
        }

        public LinkedList<Identificator> GetAllIdentificators()
        {
            return _identificators;
        }
    }
}
