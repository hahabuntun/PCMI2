using Lab2.Commands;
using Lab2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Lab2.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<FileItem> Items { get; set; }

        private ObservableCollection<FileItem> _selectedItems = new ObservableCollection<FileItem>();

        private OperationResult _operationResult = new OperationResult();
        public OperationResult OperationResult
        {
            get => _operationResult;
            set
            {
                _operationResult = value;
                OnPropertyChanged(nameof(OperationResult));
            }
        }

        public ObservableCollection<FileItem> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;
                OnPropertyChanged(nameof(SelectedItems));
            }
        }

        public void UpdateCommandStates()
        {
            ((RelayCommand)NavigateBackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CopyCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CutCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PasteCommand).RaiseCanExecuteChanged();
            OperationResult = new OperationResult { ResultTxt = "", IsError = false };
        }

        private string _currentDirectory;
        private List<string> _clipboardItems = new List<string>();
        public bool _isCutOperation;

        public ICommand NavigateBackCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand PasteCommand { get; }

        public string CurrentDirectory
        {
            get => _currentDirectory;
            set
            {
                _currentDirectory = value;
                OnPropertyChanged(nameof(CurrentDirectory));
            }
        }

        public bool CanNavigateBack => Directory.GetParent(_currentDirectory) != null;
        public bool CanCopyItems => SelectedItems.Any();
        public bool CanCutItems => SelectedItems.Any();
        public bool CanPasteItems => _clipboardItems.Any();

        public MainViewModel()
        {
            CurrentDirectory = Directory.GetCurrentDirectory();
            LoadItems(CurrentDirectory);

            NavigateBackCommand = new RelayCommand(NavigateBack, () => CanNavigateBack);
            CopyCommand = new RelayCommand(CopyItems, () => CanCopyItems);
            CutCommand = new RelayCommand(CutItems, () => CanCutItems);
            PasteCommand = new RelayCommand(PasteItems, () => CanPasteItems);
        }

        private void LoadItems(string directory)
        {
            var directories = Directory.GetDirectories(directory)
                .Select(d => new FileItem { Name = Path.GetFileName(d), IsDirectory = true, Path = d });
            var files = Directory.GetFiles(directory)
                .Select(f => new FileItem { Name = Path.GetFileName(f), IsDirectory = false, Path = f });

            Items = new ObservableCollection<FileItem>(directories.Concat(files));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(CanNavigateBack));
        }

        public void NavigateToSelectedItem()
        {
            if (SelectedItems.FirstOrDefault(item => item.IsDirectory) is FileItem selectedItem)
            {
                Trace.WriteLine($"Navigating to directory: {selectedItem.Path}");
                CurrentDirectory = selectedItem.Path;
                LoadItems(CurrentDirectory);
            }
        }

        private void NavigateBack()
        {
            var parentDirectory = Directory.GetParent(_currentDirectory)?.FullName;
            if (parentDirectory != null)
            {
                Trace.WriteLine($"Navigating back to directory: {parentDirectory}");
                CurrentDirectory = parentDirectory;
                LoadItems(CurrentDirectory);
            }
        }

        private void CopyItems()
        {
            if (SelectedItems.Any())
            {
                _clipboardItems = SelectedItems.Select(item => item.Path).ToList();
                _isCutOperation = false;
                Trace.WriteLine($"Copied items: {string.Join(", ", _clipboardItems)}");
                OnPropertyChanged(nameof(CanPasteItems));
                UpdateCommandStates();
                OperationResult = new OperationResult { ResultTxt = "Items copied to clipboard.", IsError = false };
            }
        }

        private void CutItems()
        {
            if (SelectedItems.Any())
            {
                _clipboardItems = SelectedItems.Select(item => item.Path).ToList();
                _isCutOperation = true;
                Trace.WriteLine($"Cut items: {string.Join(", ", _clipboardItems)}");
                OnPropertyChanged(nameof(CanPasteItems));
                UpdateCommandStates();
                OperationResult = new OperationResult { ResultTxt = "Items cut to clipboard.", IsError = false };
            }
        }

        private void PasteItems()
        {
            if (_clipboardItems.Any())
            {
                foreach (var clipboardItem in _clipboardItems)
                {
                    string destinationPath = Path.Combine(_currentDirectory, Path.GetFileName(clipboardItem));
                    try
                    {
                        if (IsMovingIntoChildDirectory(clipboardItem, destinationPath))
                        {
                            throw new InvalidOperationException("Cannot move parent directory into child directory.");
                        }

                        if (Directory.Exists(clipboardItem))
                        {
                            if (_isCutOperation)
                            {
                                Directory.Move(clipboardItem, destinationPath);
                                Trace.WriteLine($"Moved directory from {clipboardItem} to {destinationPath}");
                            }
                            else
                            {
                                CopyDirectory(clipboardItem, destinationPath);
                                Trace.WriteLine($"Copied directory from {clipboardItem} to {destinationPath}");
                            }
                        }
                        else if (File.Exists(clipboardItem))
                        {
                            if (_isCutOperation)
                            {
                                File.Move(clipboardItem, destinationPath);
                                Trace.WriteLine($"Moved file from {clipboardItem} to {destinationPath}");
                            }
                            else
                            {
                                File.Copy(clipboardItem, destinationPath);
                                Trace.WriteLine($"Copied file from {clipboardItem} to {destinationPath}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error during paste operation: {ex.Message}");
                        OperationResult = new OperationResult { ResultTxt = $"Error during paste operation: {ex.Message}", IsError = true };
                        return;
                    }
                }

                LoadItems(_currentDirectory);
                _clipboardItems.Clear();
                OnPropertyChanged(nameof(CanPasteItems));
                UpdateCommandStates();
                OperationResult = new OperationResult { ResultTxt = "Paste operation completed successfully.", IsError = false };
            }
        }

        public bool IsMovingIntoChildDirectory(string sourcePath, string destinationPath)
        {
            var sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            var destinationDirectoryInfo = new DirectoryInfo(destinationPath);

            bool isInside = destinationDirectoryInfo.FullName.StartsWith(sourceDirectoryInfo.FullName, StringComparison.OrdinalIgnoreCase);
            bool isSameDirectory = string.Equals(sourceDirectoryInfo.FullName, destinationDirectoryInfo.FullName, StringComparison.OrdinalIgnoreCase);

            return isInside && !isSameDirectory;
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            var dir = new DirectoryInfo(sourceDir);

            Directory.CreateDirectory(destDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }
    }
}
