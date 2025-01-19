using Microsoft.Win32;
using NLog;
using System.Globalization;
using System.Management;
using System.Reflection;

namespace AsylumLauncher.Data
{
    internal class SystemHandler
    {
        private string RegDirectory;
        public string GPUData = "";
        public string CPUData = "";

        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        public SystemHandler()
        {
            Nlog.Info("Constructor - Successfully initialized SystemHandler.");
            CPUData = InitializeCPU().ToUpper();
            GPUData = InitializeGPUValues().ToUpper();
        }

        private static string InitializeCPU()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
                var CPU = searcher.Get().Cast<ManagementObject>().FirstOrDefault() ?? throw new InvalidOperationException("No CPU information found.");
                uint Clockspeed = (uint)CPU["MaxClockSpeed"];
                double GHzSpeed = Clockspeed / 1000.0;
                Nlog.Info("InitializeCPU - Recognized CPU as {0} with a base clock speed of {1}GHz.", CPU["Name"].ToString().Trim(), Math.Round(GHzSpeed, 1));
                var CPUName = CPU["Name"].ToString().Trim();
                return CPUName.ToUpper().Contains("GHZ") ? CPUName : $"{CPUName} @ {Math.Round(GHzSpeed, 1)}GHz";
            }
            catch (Exception e)
            {
                Nlog.Error("InitializeCPU - Could not read CPU information. Error: {0}", e);
                Program.MainWindow.BasicToolTip.SetToolTip(Program.MainWindow.CPULabel, "Current date.");
                return DateTime.Now.ToString("dddd, MMMM dd, yyyy", new CultureInfo("en-GB"));
            }
        }

        private string InitializeGPUValues()
        {
            try
            {
                RegDirectory = GetRegistryDirectory();
                var vRamValue = Registry.GetValue(RegDirectory, "HardwareInformation.qwMemorySize", null);
                var VRam = ConvertVRamValue(vRamValue ?? 0);
                if (string.IsNullOrWhiteSpace(VRam))
                {
                    return SetGPUNameVideoController();
                }
                var gpuName = (string?)Registry.GetValue(RegDirectory, "DriverDesc", "Could not find GPU name.") ?? "Unknown GPU";
                Nlog.Info("InitializeGPUValues - Recognized GPU as {0} with a total VRAM amount of {1}.", gpuName, VRam);
                return $"{gpuName} {VRam}";
            }
            catch (Exception e)
            {
                Nlog.Error("InitializeGPUValues - Could not read Graphics Card information. Error: {0}", e);
                Program.MainWindow.BasicToolTip.SetToolTip(Program.MainWindow.GPULabel, "Current version.");
                return "Application Version: " + (Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version");
            }
        }

        private static string GetRegistryDirectory()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\\ControlSet001\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\0000");
            if (key == null)
            {
                return Path.Combine(Registry.LocalMachine.ToString(), "SYSTEM\\ControlSet001\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\0001");
            }
            return Path.Combine(Registry.LocalMachine.ToString(), "SYSTEM\\ControlSet001\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\0000");
        }

        private static string SetGPUNameVideoController()
        {
            List<string> GPUList = new();
            using var search = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementBaseObject o in search.Get())
            {
                using var obj = (ManagementObject)o;
                foreach (PropertyData data in obj.Properties)
                {
                    if (data.Name == "Description" && data.Value != null)
                    {
                        GPUList.Add(data.Value.ToString());
                    }
                }
            }

            if (GPUList.Count == 0)
            {
                Nlog.Warn("SetGPUNameVideoController - No GPU descriptions found.");
                return "Unknown GPU";
            }

            var GPU = GPUList[0];
            if (GPUList.Count > 1)
            {
                foreach (string s in GPUList)
                {
                    if (s.Contains("NVIDIA") || s.Contains("AMD"))
                    {
                        GPU = s;
                        break;
                    }
                }
            }
            Nlog.Warn("SetGPUNameVideoController - Used fallback method to determine GPU as {0}. Could not correctly determine VRAM amount. Your GPU drivers may be corrupted.", GPU);
            return GPU;
        }

        ///<Returns VRAM value in GB in most cases.</Returns>.</summary>
        private static string ConvertVRamValue(object VRam)
        {
            try
            {
                long VRamValue = Convert.ToInt64(VRam);

                var Affix = "MB";
                if (VRamValue >= 1073741824)
                {
                    VRamValue /= 1024;
                    Affix = "GB";
                }
                VRamValue /= 1048576;
                return $"({VRamValue}{Affix})";
            }
            catch (InvalidCastException)
            {
                return "";
            }
            catch (NullReferenceException)
            {
                return "";
            }
        }
    }
}
