using Avalonia.Controls;

namespace Translator.Services.Dialogues.Base
{
    public interface IDialogueManager
    {
        public TopLevel? GetTopLevelForContext(object context);
    }
}
