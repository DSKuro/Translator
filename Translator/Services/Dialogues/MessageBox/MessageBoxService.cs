using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;
using Translator.Models;
using Translator.Services.Dialogues.Base;

namespace Translator.Services.Dialogues.MessageBox
{
    public class MessageBoxService : IMessageBoxService
    {
        private readonly IDialogueHelper _dialogueHelper;

        public MessageBoxService(IDialogueHelper dialogueHelper)
        {
            _dialogueHelper = dialogueHelper;
        }

        public Task<ButtonResult?> ShowMessageBoxAsync(object context,
            MessageBoxOptions options)
        {
            TopLevel topLevel = _dialogueHelper.GetTopLevelForAnyDialogue(context);
            return ShowMessageBoxImpl(options, topLevel);
        }

        private async Task<ButtonResult?> ShowMessageBoxImpl(MessageBoxOptions options,
            TopLevel topLevel)
        {
            IMsBox<ButtonResult> msgBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ButtonDefinitions = options.Buttons,
                ContentTitle = options.Title,
                ContentMessage = options.Content,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true,

            });
            return await msgBox.ShowWindowDialogAsync(topLevel as Window);
        }
    }
}
