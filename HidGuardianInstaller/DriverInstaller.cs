using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using Microsoft.Win32;
using Nefarius.Devcon;

namespace HidGuardianInstaller
{
    class DriverInstaller
    {
        private double progress;
        public event EventHandler ProgressChanged;
        public double Progress
        {
            get => progress;
            set
            {
                if (progress == value) return;
                progress = value;
                ProgressChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool hidguardInstalled;
        public event EventHandler RunFinished;

        public void CheckInstall()
        {
            hidguardInstalled = Util.IsHidGuardInstalled();
        }

        public bool IsInstalled()
        {
            return hidguardInstalled;
        }

        public async void Run()
        {
            bool deviceCreated = false;

            hidguardInstalled = false;
            AppLogger.Log("Start HidGuardian install");
            Progress = 0.0;

            // Step 1
            string archiveName = "HidGuardian.zip";
            using (WebClient wb = new WebClient())
            {
                AppLogger.Log("Start downloading HidGuardian archive");
                wb.DownloadProgressChanged += UpdateDlProgress;
                await wb.DownloadFileTaskAsync(new Uri("https://downloads.vigem.org/projects/HidGuardian/stable/1.14.3.0/windows/x64/HidGuardian.zip"),
                    Path.Combine(Util.exepath, archiveName));
            }

            string archivePath = Path.Combine(Util.exepath, archiveName);
            string hidGuardPath = Path.Combine(Util.exepath, "HidGuardian");

            if (!File.Exists(archivePath))
            {
                AppLogger.Log("Failed to find HidGuardian archive file");
                Progress = 100.0;
                hidguardInstalled = false;
                return;
            }

            // Step 2
            Progress = 16.6;
            if (Directory.Exists(hidGuardPath))
            {
                Directory.Delete(hidGuardPath, true);
            }

            Directory.CreateDirectory(hidGuardPath);
            try
            {
                ZipFile.ExtractToDirectory(archivePath, hidGuardPath);
                //ZipFile.ExtractToDirectory(Path.Combine(Util.exepath, archiveName),
                //    hidGuardPath);
            } // Saved so the user can uninstall later
            catch
            {
                AppLogger.Log($"Failed to extract {archivePath}");
                Progress = 100.0;
                hidguardInstalled = false;
                return;
            }

            // Step 3
            AppLogger.Log("Creating HidGuardian Virtual Device");
            Progress = 33.3;

            deviceCreated = Devcon.Create("System", Util.sysGuid, Util.hidGuardDevicePath);
            //Console.WriteLine("SUBMIT!!!: " + result.ToString());

            // Step 4
            AppLogger.Log("Installing HidGuardian Driver");
            Progress = 50.0;

            string infPath = Path.Combine(hidGuardPath, Util.arch, "HidGuardian.inf");
            AppLogger.Log($"Installing driver from {infPath}");
            //result = Devcon.Install(@"C:\Users\ryoch\Downloads\Sources\ViGEm\x64\Release\ViGEmBus\ViGEmBus.inf", out temp);
            hidguardInstalled = Devcon.Install(infPath, out bool temp);
            //Console.WriteLine("1SUBMIT!!!: " + result.ToString());

            if (hidguardInstalled)
            {
                // Step 5
                Progress = 66.7;
                AppLogger.Log("HidGuardian Driver Installed");
                AppLogger.Log("Perform Registry Changes");
                RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters");
                if (key.GetValue("AffectedDevices", null) == null)
                {
                    AppLogger.Log("Writing template AffectedDevice list");
                    key.SetValue("AffectedDevices", Util.affectedDevs.ToArray(), RegistryValueKind.MultiString);
                }

                // Step 6
                Progress = 83.3;
                key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{745a17a0-74d3-11d0-b6fe-00a0c90f57da}", true);
                string[] filters = key.GetValue("UpperFilters") as string[];
                List<string> temper = filters != null ? new List<string>(filters) : new List<string>();
                if (!temper.Contains("HidGuardian"))
                {
                    //Console.WriteLine("FILTER NOT FOUND. SET IT UP.");
                    temper.Add("HidGuardian");
                    key.SetValue("UpperFilters", temper.ToArray(), RegistryValueKind.MultiString);
                }

                AppLogger.Log("HidGuardian is now installed\n");
                Progress = 100.0;
            }
            else
            {
                RemoveDevice();
                AppLogger.Log("HidGuardian install failed\n");
                Progress = 100.0;
            }

            RunFinished?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateDlProgress(object sender, DownloadProgressChangedEventArgs args)
        {
            AppLogger.Log($"Downloading HidGuardian: {args.ProgressPercentage.ToString()}%",
                false);
        }

        private void RemoveDevice()
        {
            string instanceId = Util.HidGuardInstanceId();
            if (!string.IsNullOrEmpty(instanceId))
            {
                Devcon.Remove(Util.sysGuid, instanceId);
            }
        }

        public void Uninstall()
        {
            bool result = false;

            string infFile = Util.HidGuardDevProp(NativeMethods.DEVPKEY_Device_DriverInfPath);

            AppLogger.Log("Start Uninstalling HidGuardian");
            Progress = 0.0;

            // Step 1
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{745a17a0-74d3-11d0-b6fe-00a0c90f57da}", true);
            string[] filters = key.GetValue("UpperFilters") as string[];
            List<string> temper = filters != null ? new List<string>(filters) : new List<string>();
            if (temper.Contains("HidGuardian"))
            {
                //Console.WriteLine("Removing HidGuardian UpperFilters Entry");
                AppLogger.Log("Removing HidGuardian UpperFilters Entry");
                temper.Remove("HidGuardian");
                key.SetValue("UpperFilters", temper.ToArray(), RegistryValueKind.MultiString);
            }

            // Step 2
            AppLogger.Log("Removing HidGuardian Virtual Device");
            Progress = 33.3;

            string instanceId = Util.HidGuardInstanceId();
            if (!string.IsNullOrEmpty(instanceId))
            {
                result = Devcon.Remove(Util.sysGuid, instanceId);
            }

            // Step 3
            AppLogger.Log("Removing HidGuardian driver from driver store");
            Progress = 66.6;

            result = NativeMethods.SetupUninstallOEMInfW(infFile, 0x0001, IntPtr.Zero);

            AppLogger.Log("Finished\n");
            Progress = 100.0;

            hidguardInstalled = false;
        }
    }
}
