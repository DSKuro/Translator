using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Translator.Models;

namespace Translator.Services.Dialogues.Storage
{
    public interface IStorageService
    {
        public Task<IEnumerable<IStorageFile>> OpenFileAsync(object context, FileOpenOptions options);
        public Task<IStorageFile> SaveFileAsync(object context, FileSaveOptions options);
    }
}
