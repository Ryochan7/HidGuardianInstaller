using System;
using System.Collections.Generic;
using System.Linq;
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

namespace HidGuardianInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DriverInstaller installer = new DriverInstaller();
        private bool initialDriverStatus;
        private bool changedDriverStatus;

        public MainWindow()
        {
            InitializeComponent();
            AppLogger.LogEvent += (message) =>
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    logMsgBox.AppendText($"{DateTime.Now.ToString()}: {message}\n");
                    progressLb.Content = message.TrimEnd();
                }));
            };

            driverProgressBar.DataContext = installer;
            installBtn.IsEnabled = false;
            uninstallBtn.IsEnabled = false;
            InitialDriverCheck();
            logMsgBox.ScrollToEnd();
        }

        private async void InitialDriverCheck()
        {
            logMsgBox.AppendText($"{DateTime.Now.ToString()}: Checking if HidGuardian is already installed\n");
            await Task.Run(() => installer.CheckInstall());
            bool installed = installer.IsInstalled();
            initialDriverStatus = installed;
            installBtn.IsEnabled = !installed;
            uninstallBtn.IsEnabled = installed;
            if (installed)
            {
                logMsgBox.AppendText($"{DateTime.Now.ToString()}: HidGuardian is already installed\n");
            }
            else
            {
                logMsgBox.AppendText($"{DateTime.Now.ToString()}: HidGuardian is not installed\n");
            }
        }

        private void InstallButtonClick(object sender, RoutedEventArgs e)
        {
            installBtn.IsEnabled = false;

            installer.RunFinished += PostInstallCheck;
            Task.Run(() => installer.Run());
        }

        private void PostInstallCheck(object sender, EventArgs args)
        {
            installer.RunFinished -= PostInstallCheck;

            Dispatcher.BeginInvoke((Action)(() =>
            {
                bool installed = installer.IsInstalled();
                installBtn.IsEnabled = !installed;
                InstallCheckRefresh();
            }));
        }

        private void InstallCheckRefresh()
        {
            installer.CheckInstall();
            bool installed = installer.IsInstalled();
            changedDriverStatus = changedDriverStatus || (installed != initialDriverStatus);
            installBtn.IsEnabled = !installed;
            uninstallBtn.IsEnabled = installed;
        }

        private void LogMsgBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            logMsgBox.ScrollToEnd();
        }

        private void UninstallBtn_Click(object sender, RoutedEventArgs e)
        {
            uninstallBtn.IsEnabled = false;
            UninstallDriver();
        }

        private async void UninstallDriver()
        {
            await Task.Run(() =>
            {
                installer.Uninstall();
            });

            InstallCheckRefresh();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (changedDriverStatus)
            {
                MessageBoxResult result = MessageBox.Show("HidGuardian installation was changed this session.\nA reboot is necessary in order for HidGuardian to be used by Windows.\nWould you like to reboot your system now?",
                    "HidGuardian Install Utility", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("shutdown", "/r /t 5");
                }
            }
        }
    }
}
