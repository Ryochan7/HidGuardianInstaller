using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HidGuardianInstaller
{
    static class Util
    {
        public static Guid sysGuid = new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}");
        public static string exepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        public static string arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        internal const string hidGuardDevicePath = @"Root\HidGuardian";
        internal const string HIDGUARD_VERSION = "1.14.3.1";

        public static List<string> affectedDevs = new List<string>()
        {
            @"HID\VID_054C&PID_05C4",
            @"HID\VID_054C&PID_09CC&MI_03",
            @"HID\VID_054C&PID_0BA0&MI_03",
            @"HID\{00001124-0000-1000-8000-00805f9b34fb}_VID&0002054c_PID&05c4",
            @"HID\{00001124-0000-1000-8000-00805f9b34fb}_VID&0002054c_PID&09cc",
        };

        public static bool IsHidGuardInstalled()
        {
            return CheckForSysDevice(hidGuardDevicePath);
        }

        private static bool CheckForSysDevice(string searchHardwareId)
        {
            bool result = false;
            Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            //var type = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (int i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                //if (NativeMethods.SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, NativeMethods.SPDRP_DEVICEDESC, ref type,
                //    dataBuffer, dataBuffer.Length, ref requiredSize))
                {
                    string hardwareId = dataBuffer.ToUTF16String();
                    if (hardwareId.Equals(searchHardwareId))
                        result = true;

                    //Console.WriteLine(dataBuffer.ToUTF8String());
                    //Console.WriteLine(hardwareId);
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        public static string HidGuardInstanceId()
        {
            return ObtainSysDeviceInstanceId(hidGuardDevicePath);
        }

        public static string HidGuardVersion()
        {
            return DeviceVersionNumber(hidGuardDevicePath);
        }

        public static string HidGuardDevProp(NativeMethods.DEVPROPKEY prop)
        {
            return GetDriverProperty(hidGuardDevicePath, prop);
        }

        private static string ObtainSysDeviceInstanceId(string searchHardwareId)
        {
            string result = "";
            bool devmatch = false;
            Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            //var type = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (int i = 0; !devmatch && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    string hardwareId = dataBuffer.ToUTF16String();
                    if (hardwareId.Equals(searchHardwareId))
                        devmatch = true;
                }
            }

            if (devmatch)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_InstanceId, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    result = dataBuffer.ToUTF16String();
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        private static string DeviceVersionNumber(string searchHardwareId)
        {
            string result = "";
            bool devmatch = false;
            Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            //var type = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (int i = 0; !devmatch && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    string hardwareId = dataBuffer.ToUTF16String();
                    if (hardwareId.Equals(searchHardwareId))
                        devmatch = true;
                }
            }

            if (devmatch)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_DriverVersion, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    result = dataBuffer.ToUTF16String();
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        private static string GetDriverProperty(string searchHardwareId,
            NativeMethods.DEVPROPKEY prop)
        {
            string result = "";
            bool devmatch = false;
            Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            //var type = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (int i = 0; !devmatch && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    string hardwareId = dataBuffer.ToUTF16String();
                    if (hardwareId.Equals(searchHardwareId))
                        devmatch = true;
                }
            }

            if (devmatch)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    result = dataBuffer.ToUTF16String();
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        public static bool IsTestSigningEnabled()
        {
            bool result = false;
            string foundGUID = string.Empty;
            using (RegistryKey key = Registry.LocalMachine.
                OpenSubKey(@"BCD00000000\Objects\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\Elements\23000003"))
            {
                if (key != null)
                {
                    foundGUID = key.GetValue("Element", string.Empty).ToString();
                }
            }

            if (!string.IsNullOrEmpty(foundGUID))
            {
                using (RegistryKey key = Registry.LocalMachine.
                    OpenSubKey($@"BCD00000000\Objects\{foundGUID}\Elements\16000049"))
                {
                    if (key != null)
                    {
                        byte[] tempAr = (byte[])key.GetValue("Element", new byte[] { 0 });
                        int temp = Convert.ToInt32(tempAr[0]);
                        if (temp > 0) result = true;
                    }
                }
            }

            return result;
        }
    }
}
