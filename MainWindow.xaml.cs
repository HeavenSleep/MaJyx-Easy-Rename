using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;

namespace MaJyxER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        #region Variables declaration
        public ObservableCollection<FileAction> matches { get; set; }
        private FilesManager manager;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            matches = new ObservableCollection<FileAction>();
            manager = new FilesManager();
            manager.MatchFound += MainWindow_MatchFound;
            manager.SearchFinished += MainWindow_SearchFinished;
            manager.ApplyDone += MainWindow_ApplyDone;
            manager.OnMessageReceive += MainWindow_OnMessageReceive;
            manager.OnCurrentFileChange += MainWindow_OnCurrentFileChange;
            manager.OnProcessActionDone += MainWindow_OnProcessActionDone;

            DataContext = this;
        }

        /// <summary>
        /// Log message into Console tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void LogMessage(object sender, string message)
        {
            string date = DateTime.Now.ToString();

            if (!Console.Dispatcher.CheckAccess())
            {
                Console.Dispatcher.Invoke(() =>
                {
                    Console.Text += $"[{date}][{sender}] {message}\n";
                });
            }
            else
            {
                Console.Text += $"[{date}][{sender}] {message}\n";
            }
        }

        /// <summary>
        /// Start cleaning impacted folders
        /// </summary>
        private void CleanFolders()
        {
            MessageBoxResult result = MessageBox.Show("Modifications have been applied\nDo you wish to clean the impacted folders if necessary?", "Clean folders?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
                manager.Clean();
        }

        /// <summary>
        /// Handle proccessed files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="fileAction"></param>
        private void MainWindow_OnProcessActionDone(object sender, FileAction fileAction)
        {
            LogMessage(sender, fileAction.Destination);

            if (!ChangeList.Dispatcher.CheckAccess())
            {
                ChangeList.Dispatcher.Invoke(() =>
                {
                    ChangeList.Items.Refresh();
                });
            }
            else
            {
                ChangeList.Items.Refresh();
            }
        }

        /// <summary>
        /// Update file progression
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="filename"></param>
        private void MainWindow_OnCurrentFileChange(object sender, string filename)
        {
            string display = filename;
            if (filename.Length > 50)
            {
                display = "..." + filename.Substring(filename.Length - 47, 47);
            }

            if (!LabelCurrent.Dispatcher.CheckAccess())
            {
                LabelCurrent.Dispatcher.Invoke(() =>
                {
                    LabelCurrent.Content = $"Current: {display}";
                    LabelCurrent.ToolTip = filename;
                });
            }
            else
            {
                LabelCurrent.Content = $"Current: {display}";
                LabelCurrent.ToolTip = filename;
            }
        }

        /// <summary>
        /// Received a message to be logged to the console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void MainWindow_OnMessageReceive(object sender, string message)
        {
            LogMessage(sender, message);
        }

        /// <summary>
        /// Received event that indicate all modifications are done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="count"></param>
        private void MainWindow_ApplyDone(object sender, int count)
        {
            if (!ButtonSearch.Dispatcher.CheckAccess())
            {
                ButtonSearch.Dispatcher.Invoke(() =>
                {
                    ButtonSearch.IsEnabled = true;
                });
            } else
            {
                ButtonSearch.IsEnabled = true;
            }

            if (!ButtonApply.Dispatcher.CheckAccess())
            {
                ButtonApply.Dispatcher.Invoke(() =>
                {
                    ButtonApply.IsEnabled = true;
                });
            } else
            {
                ButtonApply.IsEnabled = true;
            }

            LogMessage(this, $"[INFO] Apply done");

            if (!LabelCurrent.Dispatcher.CheckAccess())
            {
                LabelCurrent.Dispatcher.Invoke(() =>
                {
                    LabelCurrent.Content = "";
                });
            }
            else
            {
                LabelCurrent.Content = "";
            }

            // Handle clean process after apply
            if (ToggleCleanFolders.IsOn)
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() =>
                    {
                        CleanFolders();
                    });
                } else
                {
                    CleanFolders();
                }
            }
        }

        /// <summary>
        /// Received event that indicate that the search has concluded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="count"></param>
        private void MainWindow_SearchFinished(object sender, int count)
        {
            if (!ButtonSearch.Dispatcher.CheckAccess())
            {
                ButtonSearch.Dispatcher.Invoke(() =>
                {
                    ButtonSearch.IsEnabled = true;
                });
            } else
            {
                ButtonSearch.IsEnabled = true;
            }

            if (!ButtonApply.Dispatcher.CheckAccess())
            {
                ButtonApply.Dispatcher.Invoke(() =>
                {
                    ButtonApply.IsEnabled = true;
                });
            } else
            {
                ButtonApply.IsEnabled = true;
            }

            LogMessage(sender, $"[INFO] Search finished ({count} files scanned)");

            if (!SearchProgress.Dispatcher.CheckAccess())
            {
                SearchProgress.Dispatcher.Invoke(() =>
                {
                    SearchProgress.IsActive = false;
                });
            } else
            {
                SearchProgress.IsActive = false;
            }

            if (!LabelCurrent.Dispatcher.CheckAccess())
            {
                LabelCurrent.Dispatcher.Invoke(() =>
                {
                    LabelCurrent.Content = "";
                });
            }
            else
            {
                LabelCurrent.Content = "";
            }
        }

        /// <summary>
        /// Received a new match
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="match"></param>
        private void MainWindow_MatchFound(object sender, FileAction match)
        {
            if (!ChangeList.Dispatcher.CheckAccess())
            {
                ChangeList.Dispatcher.Invoke(() =>
                {
                    matches.Add(match);
                });
            } else
            {
                matches.Add(match);
            }

            if(!LabelMatchCount.Dispatcher.CheckAccess())
            {
                LabelMatchCount.Dispatcher.Invoke(() =>
                {
                    LabelMatchCount.Content = matches.Count().ToString();
                });
            } else
            {
                LabelMatchCount.Content = matches.Count().ToString();
            }

            LogMessage(sender, $"[INFO] Found {match.Source} to be renamed to {match.Destination}");
        }

        /// <summary>
        /// Start browsing dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog browse = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (!(bool)browse.ShowDialog())
            {
                return;
            }
            TextPath.Text = browse.SelectedPath;
        }

        /// <summary>
        /// Start a new search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            Rules rules = new Rules() { Source = TextPath.Text, Regex = TextRegex.Text, MatchAllPath = ToggleMatchMode.IsOn, Recursive = ToggleRecursive.IsOn, CleanFolders = ToggleCleanFolders.IsOn };

            if (!rules.Validate())
            {
                MessageBox.Show("Rules are not valide, please check", "Bad rules", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ButtonSearch.IsEnabled = false;
            ButtonApply.IsEnabled = false;

            matches.Clear();
            LabelMatchCount.Content = "0";

            LogMessage(this, "[INFO] Search started");
            SearchProgress.IsActive = true;

            try
            {
                await manager.Search(rules, rules.Source);
            } catch(Exception ex)
            {
                LogMessage(this, $"[ERROR] An exception occured!\n{ex}\n");
                MessageBox.Show($"Exception: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                ButtonSearch.IsEnabled = true;
            }
        }

        /// <summary>
        /// Apply pending modifications
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            ButtonSearch.IsEnabled = false;
            ButtonApply.IsEnabled = false;

            await manager.Apply(matches.ToList());
        }
    }
}
