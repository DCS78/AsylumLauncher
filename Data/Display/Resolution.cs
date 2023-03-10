﻿using NLog;
using System.Runtime.InteropServices;

namespace AsylumLauncher
{
    internal class Resolution
    {

        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();


        // string list to store resolution values
        public static List<string>? ResolutionList = new();

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(
            string deviceName, int modeNum, ref DEVMODE devMode);

        /// <summary>
        ///     Getter for user resolutions.
        ///     Called by Program upon application start.
        /// </summary>
        public void GetResolutions()
        {
            ResolutionList.Clear();
            List<string> TempList = new();
            DEVMODE vDevMode = new();
            int i = 0;
            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                TempList.Add(vDevMode.dmPelsWidth + "x" + vDevMode.dmPelsHeight);
                i++;
            }

            if (TempList.Count > 1)
            {
                int MaxLength = TempList.Max(x => x.Length);
                IOrderedEnumerable<string> OrderedList = TempList.OrderBy(x => x.PadLeft(MaxLength, '0'));
                ResolutionList = OrderedList.Distinct().ToList();
            }
            else
            {
                ResolutionList = TempList;
            }
            Nlog.Debug("GetResolutions - found a total of {0} available resolutions.", ResolutionList.Count);
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string? ToString()
        {
            return base.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int Cchdevicename = 0x20;
            private const int Cchformname = 0x20;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;

            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;

            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
    }

}
