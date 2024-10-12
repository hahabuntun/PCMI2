using Lab2.Models;
using Lab2.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lab2.Views
{
    public partial class CommandLineView : UserControl
    {
        private MainViewModel _viewModel;

        public CommandLineView()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string command = CommandInput.Text;
                ExecuteCommand(command);
                CommandInput.Clear();
            }
        }

        private void ExecuteCommand(string command)
        {
            _viewModel.OperationResult = new OperationResult(); // Сброс предыдущего результата
            CommandOutput.Items.Clear();
            try
            {
                if (command.ToLower() == "cd c" || command.ToLower() == "cd c:" || command.ToLower() == "cd d:" || command.ToLower() == "cd d")
                {
                    throw new InvalidOperationException("invalid path");
                }
                if (command.StartsWith("cd "))
                {
                    // Переход в директорию
                    string path = command.Substring(3).Trim();
                    if (path == "..")
                    {
                        _viewModel.CurrentDirectory = Directory.GetParent(_viewModel.CurrentDirectory)?.FullName ?? _viewModel.CurrentDirectory;
                    }
                    else
                    {
                        string targetDir = Path.IsPathRooted(path) ? path : Path.Combine(_viewModel.CurrentDirectory, path);
                        if (Directory.Exists(targetDir))
                        {
                            _viewModel.CurrentDirectory = targetDir;
                        }
                        else
                        {
                            throw new DirectoryNotFoundException($"Directory not found: {path}");
                        }
                    }
                }
                else if (command.StartsWith("ls"))
                {
                    DisplayDirectoryContents();
                }
                else if (command.StartsWith("copy "))
                {
                    // Копирование файлов/директорий
                    string[] args = command.Substring(5).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length < 2)
                        throw new ArgumentException("Invalid copy command. Usage: copy <source> <destination>");

                    string destPath = Path.IsPathRooted(args.Last()) ? args.Last() : Path.Combine(_viewModel.CurrentDirectory, args.Last());
                    for (int i = 0; i < args.Length - 1; i++)
                    {
                        string sourcePath = Path.IsPathRooted(args[i]) ? args[i] : Path.Combine(_viewModel.CurrentDirectory, args[i]);
                        if (File.Exists(sourcePath))
                        {
                            string destination = Path.Combine(destPath, Path.GetFileName(sourcePath));
                            File.Copy(sourcePath, destination);
                        }
                        else if (Directory.Exists(sourcePath))
                        {
                            string destination = Path.Combine(destPath, Path.GetFileName(sourcePath));
                            if (_viewModel.IsMovingIntoChildDirectory(sourcePath, destination))
                            {
                                throw new InvalidOperationException("Cannot move parent directory into child directory.");
                            }
                            CopyDirectory(sourcePath, destination);
                        }
                        else
                        {
                            throw new FileNotFoundException($"File or directory not found: {sourcePath}");
                        }
                    }
                }
                else if (command.StartsWith("help"))
                {
                    Help();
                }
                else if (command.StartsWith("cut "))
                {
                    // Вырезание (перемещение) файлов/директорий
                    string[] args = command.Substring(4).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length < 2)
                        throw new ArgumentException("Invalid cut command. Usage: cut <source> <destination>");

                    string destPath = Path.IsPathRooted(args.Last()) ? args.Last() : Path.Combine(_viewModel.CurrentDirectory, args.Last());
                    for (int i = 0; i < args.Length - 1; i++)
                    {
                        string sourcePath = Path.IsPathRooted(args[i]) ? args[i] : Path.Combine(_viewModel.CurrentDirectory, args[i]);
                        if (File.Exists(sourcePath))
                        {
                            string destination = Path.Combine(destPath, Path.GetFileName(sourcePath));
                            File.Move(sourcePath, destination);
                        }
                        else if (Directory.Exists(sourcePath))
                        {
                            string destination = Path.Combine(destPath, Path.GetFileName(sourcePath));
                            if (_viewModel.IsMovingIntoChildDirectory(sourcePath, destination))
                            {
                                throw new InvalidOperationException("Cannot move parent directory into child directory.");
                            }
                            Directory.Move(sourcePath, destination);
                        }
                        else
                        {
                            throw new FileNotFoundException($"File or directory not found: {sourcePath}");
                        }
                    }
                }
                else
                {
                    _viewModel.OperationResult = new OperationResult { ResultTxt = $"Unknown command: {command}", IsError = true };
                }
            }
            catch (Exception ex)
            {
                _viewModel.OperationResult = new OperationResult { ResultTxt = $"Error: {ex.Message}", IsError = true };
            }
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string targetFilePath = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string newDestinationDir = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDirectory(directory, newDestinationDir);
            }
        }

        private void DisplayDirectoryContents()
        {
            CommandOutput.Items.Clear(); // Очищаем предыдущий вывод
            var dirs = Directory.GetDirectories(_viewModel.CurrentDirectory);
            var files = Directory.GetFiles(_viewModel.CurrentDirectory);

            CommandOutput.Items.Add("Directories:");
            foreach (var dir in dirs)
            {
                var item = new ListBoxItem
                {
                    Content = $"{Path.GetFileName(dir)} [Directory]",
                    Foreground = Brushes.Blue
                };
                CommandOutput.Items.Add(item);
            }

            CommandOutput.Items.Add("Files:");
            foreach (var file in files)
            {
                var item = new ListBoxItem
                {
                    Content = $"{Path.GetFileName(file)} [File]",
                    Foreground = Brushes.Red
                };
                CommandOutput.Items.Add(item);
            }
        }

        private void Help()
        {
            CommandOutput.Items.Clear(); // Очищаем предыдущий вывод
            CommandOutput.Items.Add("cut имя_файла(директории) | имя_файла(директории) ... куда(в какую директорию)");
            CommandOutput.Items.Add("copy имя_файла(директории) | имя_файла(директории) ... куда(в какую директорию)");
            CommandOutput.Items.Add("help для получения помощи");
            CommandOutput.Items.Add("ls - получение данных о директории текущей");
            CommandOutput.Items.Add("cd путь для перехода по директориям");
        }
    }
}
