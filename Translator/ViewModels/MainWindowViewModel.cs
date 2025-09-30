using Avalonia.Platform.Storage;
using ClassLibrary.Files;
using ClassLibrary.Lexems.Exceptions;
using ClassLibrary.Syntax;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const string TASM_PATH = "D:\\API\\DOSBox-0.74-3";
        private const string CODE_PATH = "D:\\API\\DOSBox-0.74-3\\ASM\\code.asm";

        private readonly IStorageService _storageService;

        private bool _isSuccesful = false;
        private Reader _reader;
        private SyntaxAnalyser _analyser;

        [ObservableProperty]
        private string _originalCode;
        [ObservableProperty]
        private string _resultCode;
        [ObservableProperty]
        private string _compilationCode;

        public MainWindowViewModel(IMessageBoxService messageBoxService,
            IStorageService storageService) 
            : base(messageBoxService) 
        { 
            _storageService = storageService;
        }

        [RelayCommand]
        public async Task OpenFileCommand() 
        {
            try 
            {
                _isSuccesful = false;
                OriginalCode = "";
                ResultCode = "";
                CompilationCode = "";
                if (_reader != null)
                {
                    _reader.Dispose();
                }
                IEnumerable<IStorageFile> fileProperties = await _storageService.OpenFileAsync("MainWindow", 
                    new FileOpenOptions(StorageOpenConstants.OpenBaseTextFile.Value,
                    StorageOpenConstants.OpenBaseTextFile.Type));
                if (fileProperties.Count() > 0)
                {
                    if (File.Exists(fileProperties.FirstOrDefault().Path.AbsolutePath))
                    {
                        OriginalCode = Reader.ReadAllFile(fileProperties.FirstOrDefault().Path.AbsolutePath);
                    }
                    else
                    {
                        await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                            MessageBoxConstants.Error.Value, "Файл не существует",
                            ButtonEnum.Ok));
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Аргумент не найден",
                    ButtonEnum.Ok));
            }
            catch (ArgumentException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Неверный аргумент",
                    ButtonEnum.Ok));
            }
            catch (InvalidOperationException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Запрещённая операция",
                    ButtonEnum.Ok));
            }
            catch (PathTooLongException ex) 
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Путь к файлу слишком длинный",
                    ButtonEnum.Ok));
            }
            catch (FileNotFoundException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Файл не найден",
                    ButtonEnum.Ok));
            }
            catch (UnauthorizedAccessException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Нет доступа",
                    ButtonEnum.Ok));
            }
            catch (IOException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Ошибка чтения",
                    ButtonEnum.Ok));
            }
            return;
        }

        [RelayCommand]
        public async Task SaveFileCommand()
        {
            try
            {
                if (ResultCode == "")
                {
                    await MessageBoxHelper("MainWindow",
                        new MessageBoxOptions(MessageBoxConstants.Error.Value,
                        "Нет результата", ButtonEnum.Ok));
                    return;
                } 
                IStorageFile fileProperties = await _storageService.SaveFileAsync("MainWindow",
                    new FileSaveOptions(StorageSaveConstants.OpenBaseTextFile.ShowPrompt,
                    StorageSaveConstants.OpenBaseTextFile.Extension,
                    StorageSaveConstants.OpenBaseTextFile.Value,
                    StorageSaveConstants.OpenBaseTextFile.Types));
                if (fileProperties != null)
                {
                    using (Writer writer = new Writer(true, fileProperties.Path.AbsolutePath))
                    {
                        writer.WriteToFile(ResultCode);
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Путь указан неверно",
                    ButtonEnum.Ok));
            }
            catch (ArgumentNullException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Аргумент не найден",
                    ButtonEnum.Ok));
            }
            catch (ArgumentException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Неверный аргумент",
                    ButtonEnum.Ok));
            }
            catch (InvalidOperationException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Запрещённая операция",
                    ButtonEnum.Ok));
            }
            catch (PathTooLongException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Путь к файлу слишком длинный",
                    ButtonEnum.Ok));
            }
            catch (FileNotFoundException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Файл не найден",
                    ButtonEnum.Ok));
            }
            catch (UnauthorizedAccessException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Нет доступа",
                    ButtonEnum.Ok));
            }
            catch (IOException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Ошибка чтения",
                    ButtonEnum.Ok));
            }
            return;
        }

        [RelayCommand]
        public async Task CompileCommand()
        {
            try
            {
                ResultCode = "";
                if (OriginalCode == "")
                {
                    await MessageBoxHelper("MainWindow",
                        new MessageBoxOptions(
                            MessageBoxConstants.Error.Value,
                            "Код не определён", ButtonEnum.Ok));
                    return;
                }

                if (_reader != null)
                {
                    _reader.Dispose();
                }

                _reader = new Reader(OriginalCode);

                _analyser = new SyntaxAnalyser(_reader);

                if (!(_isSuccesful = _analyser.Compile()))
                {
                    string errors = _analyser.ErrorsToString();
                    if (errors != null)
                    {
                        CompilationCode = errors;
                    }
                }
                else
                {
                    CompilationCode = "Синтаксический анализатор: ошибок не обнаружено";
                    ResultCode = _analyser.GetCommands();
                }
            } 
            catch (LexicalException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                   MessageBoxConstants.Error.Value, ex.Message,
                    ButtonEnum.Ok));
            }
            catch (ArgumentNullException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Аргумент не найден",
                    ButtonEnum.Ok));
            }
            catch (ArgumentException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Неверный аргумент",
                    ButtonEnum.Ok));
            }
            catch (InvalidOperationException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Запрещённая операция",
                    ButtonEnum.Ok));
            }
            catch (PathTooLongException ex) 
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Путь к файлу слишком длинный",
                    ButtonEnum.Ok));
            }
            catch (FileNotFoundException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Файл не найден",
                    ButtonEnum.Ok));
            }
            catch (UnauthorizedAccessException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Нет доступа",
                    ButtonEnum.Ok));
            }
            catch (IOException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Ошибка чтения",
                    ButtonEnum.Ok));
            }
        }

        [RelayCommand]
        public async Task RunTranslator()
        {
            try
            {
                if (_isSuccesful)
                {
                    using (Writer write = new Writer(false, CODE_PATH))
                    {
                        write.WriteToFile(ResultCode);
                    }
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo(TASM_PATH + "\\DOSBox.exe",
                        TASM_PATH + "\\ASM\\code.bat -noconsole");
                    process.Start();
                }
                else
                {
                    await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                        MessageBoxConstants.Error.Value, "Невозможно выполнить",
                        ButtonEnum.Ok));
                }
            }
            catch (LexicalException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                   MessageBoxConstants.Error.Value, ex.Message,
                    ButtonEnum.Ok));
            }
            catch (ArgumentNullException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Аргумент не найден",
                    ButtonEnum.Ok));
            }
            catch (ArgumentException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Неверный аргумент",
                    ButtonEnum.Ok));
            }
            catch (InvalidOperationException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Запрещённая операция",
                    ButtonEnum.Ok));
            }
            catch (PathTooLongException ex) 
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Путь к файлу слишком длинный",
                    ButtonEnum.Ok));
            }
            catch (FileNotFoundException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Файл не найден",
                    ButtonEnum.Ok));
            }
            catch (UnauthorizedAccessException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Нет доступа",
                    ButtonEnum.Ok));
            }
            catch (IOException ex)
            {
                await MessageBoxHelper("MainWindow", new MessageBoxOptions(
                    MessageBoxConstants.Error.Value, "Ошибка чтения",
                    ButtonEnum.Ok));
            }
        }
    }
}
