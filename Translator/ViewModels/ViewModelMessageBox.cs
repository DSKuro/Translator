using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;
using Translator.Models;
using Translator.Services.Dialogues.MessageBox;

namespace Translator.ViewModels
{
    public class ViewModelMessageBox : ViewModelBase
    {
        private readonly IMessageBoxService _messageBoxService;

        public ViewModelMessageBox(IMessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService;
        }

        protected async Task<ButtonResult?> MessageBoxHelper(object context, MessageBoxOptions options, Action callback)
        {
            ButtonResult? result = null;
            try
            {
                result = await _messageBoxService.ShowMessageBoxAsync(context, options);
            }
            finally
            {
                if (callback != null)
                {
                    callback.Invoke();
                }
            }
            return result;
        }

        protected void ErrorCallback()
        {
            Environment.Exit(1);
        }
    }
}
