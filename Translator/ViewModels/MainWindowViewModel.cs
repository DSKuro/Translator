using Avalonia.Platform.Storage;
using ClassLibrary.Files.Interfaces;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Translator.Models;
using Translator.Services.Dialogues.MessageBox;
using Translator.Services.Dialogues.Storage;

namespace Translator.ViewModels
{
    public partial class MainWindowViewModel : ViewModelMessageBox
    {
        private readonly IStorageService _storageService;
        private readonly IReaderFactory _readerFactory;

        public MainWindowViewModel(IMessageBoxService messageBoxService,
            IStorageService storageService,
            IReaderFactory readerFactory) 
            : base(messageBoxService) 
        { 
            _storageService = storageService;
            _readerFactory = readerFactory;
        }

        [RelayCommand]
        public async Task OpenFileCommand() 
        {
            try 
            {
                IEnumerable<IStorageFile> fileProperties = await _storageService.OpenFileAsync("MainWindow", 
                    new FileOpenOptions(StorageOpenConstants.OpenBaseTextFile.Value,
                    StorageOpenConstants.OpenBaseTextFile.Type));
                using (IReader reader = _readerFactory.CreateReader(fileProperties.First().Path.AbsolutePath))
                {
                    char symbol;
                    do
                    {
                        symbol = reader.ReadNextSymbol();
                    } while (symbol != -1);
                }
            }
            catch (ArgumentNullException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Аргумент не найден",
                    ButtonEnum.Ok), null);
            }
            catch (ArgumentException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Неверный аргумент",
                    ButtonEnum.Ok), null);
            }
            catch (InvalidOperationException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Запрещённая операция",
                    ButtonEnum.Ok), null);
            }
            catch (PathTooLongException ex) 
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Путь к файлу слишком длинный",
                    ButtonEnum.Ok), null);
            }
            catch (FileNotFoundException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Файл не найден",
                    ButtonEnum.Ok), null);
            }
            catch (UnauthorizedAccessException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Нет доступа",
                    ButtonEnum.Ok), null);
            }
            catch (IOException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Ошибка чтения",
                    ButtonEnum.Ok), null);
            }
            return;
        }

        [RelayCommand]
        public async Task SaveFileCommand()
        {
            try
            {
                IStorageFile fileProperties = await _storageService.SaveFileAsync("MainWindow",
                    new FileSaveOptions(StorageSaveConstants.OpenBaseTextFile.ShowPrompt,
                    StorageSaveConstants.OpenBaseTextFile.Extension,
                    StorageSaveConstants.OpenBaseTextFile.Value,
                    StorageSaveConstants.OpenBaseTextFile.Types));
            }
            catch (ArgumentNullException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Аргумент не найден",
                    ButtonEnum.Ok), null);
            }
            catch (ArgumentException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Неверный аргумент",
                    ButtonEnum.Ok), null);
            }
            catch (InvalidOperationException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Запрещённая операция",
                    ButtonEnum.Ok), null);
            }
            catch (PathTooLongException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Путь к файлу слишком длинный",
                    ButtonEnum.Ok), null);
            }
            catch (FileNotFoundException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Файл не найден",
                    ButtonEnum.Ok), null);
            }
            catch (UnauthorizedAccessException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Нет доступа",
                    ButtonEnum.Ok), null);
            }
            catch (IOException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Ошибка чтения",
                    ButtonEnum.Ok), null);
            }
            return;
        }
    }
}
