namespace Translator.Services.Dialogues.MessageBox
{
    public class MessageBoxConstants
    {
        public string Value { get; private set; }

        private MessageBoxConstants(string value)
        {
            Value = value;
        }

        public static MessageBoxConstants Error { get { return new MessageBoxConstants("Ошибка"); } }
    }
}
