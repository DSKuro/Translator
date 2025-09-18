using Avalonia.Controls;
using System;

namespace Translator.Services.Dialogues.Base
{
    public class DialogueHelper : IDialogueHelper
    {
        private readonly IDialogueManager _dialogueManager;

        public DialogueHelper(IDialogueManager dialogueManager)
        {
            _dialogueManager = dialogueManager;
        }

        public TopLevel GetTopLevelForAnyDialogue(object? context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            TopLevel? topLevel = _dialogueManager.GetTopLevelForContext(context);

            if (topLevel == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return topLevel;
        }
    }
}
