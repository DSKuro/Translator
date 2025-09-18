using Avalonia.Controls;

namespace Translator.Services.Dialogues.Base
{
    public interface IDialogueHelper
    {
        public TopLevel GetTopLevelForAnyDialogue(object? context);
    }
}
