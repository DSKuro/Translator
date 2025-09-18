using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using Translator.Models;

namespace Translator.Services.Dialogues.MessageBox
{
    public interface IMessageBoxService
    {
        public Task<ButtonResult?> ShowMessageBoxAsync(object context, MessageBoxOptions options);
    }
}
