using Lab2.ViewModels;
using System.Diagnostics; // Add this
using System.Windows.Controls;
using System.Windows.Input;

namespace Lab2.Views
{
    public partial class FileExplorerView : UserControl
    {
        public FileExplorerView()
        {
            InitializeComponent();
            this.KeyDown += FileExplorerView_KeyDown;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            Trace.WriteLine("ListView item double-clicked."); // Log action
            viewModel?.NavigateToSelectedItem();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel == null)
                return;

            viewModel.SelectedItems.Clear();
            foreach (FileItem item in ((ListView)sender).SelectedItems)
            {
                viewModel.SelectedItems.Add(item);
            }

            viewModel.UpdateCommandStates();
            Trace.WriteLine($"Selected items changed: {string.Join(", ", viewModel.SelectedItems)}"); // Log selection change
        }

        private void FileExplorerView_KeyDown(object sender, KeyEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel == null)
                return;

            // Handle hotkeys
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (viewModel.CanCopyItems)
                {
                    viewModel.CopyCommand.Execute(null);
                    Trace.WriteLine("Executed Copy command."); // Log action
                }
            }
            else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (viewModel.CanPasteItems)
                {
                    viewModel.PasteCommand.Execute(null);
                    Trace.WriteLine("Executed Paste command."); // Log action
                }
            }
            else if (e.Key == Key.X && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (viewModel.CanCutItems)
                {
                    viewModel.CutCommand.Execute(null);
                    Trace.WriteLine("Executed Cut command."); // Log action
                }
            }
        }
    }
}
