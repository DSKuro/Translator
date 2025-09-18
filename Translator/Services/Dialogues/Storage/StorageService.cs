using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Translator.Models;
using Translator.Services.Dialogues.Base;

namespace Translator.Services.Dialogues.Storage
{
    public class StorageService : IStorageService
    {
        private readonly IDialogueHelper _dialogueHelper;

        public StorageService(IDialogueHelper dialogueHelper)
        {
            _dialogueHelper = dialogueHelper;
        }

        public async Task<IEnumerable<IStorageFile>> OpenFileAsync(object context, FileOpenOptions options)
        {
            TopLevel topLevel = _dialogueHelper.GetTopLevelForAnyDialogue(context);
            return await OpenFilesImpl(context, options, topLevel);
        }

        private async Task<IEnumerable<IStorageFile>> OpenFilesImpl(object context,
                            FileOpenOptions options,
                            TopLevel topLevel)
        {
            IReadOnlyList<IStorageFile> storageFiles = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions()
                {
                    AllowMultiple = options.AllowMultiple,
                    Title = options.Title ?? "Выберите файл(ы)",
                    FileTypeFilter = new[] { options.Filter }
                }
            );
            return storageFiles;
        }

        public async Task<IStorageFile> SaveFileAsync(object context, FileSaveOptions options)
        {
            TopLevel topLevel = _dialogueHelper.GetTopLevelForAnyDialogue(context);
            return await SaveFileImpl(context, options, topLevel);
        }

        private async Task<IStorageFile> SaveFileImpl(object context, 
            FileSaveOptions options,
            TopLevel topLevel)
        {
            IStorageFile? storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    ShowOverwritePrompt = options.ShowOverwritePrompt,
                    DefaultExtension = options.Extension,
                    Title = options.Title,
                    FileTypeChoices = options.Types,
                });
            return storageFile;
        }
    }
}
