using AsylumLauncher.Data.Display;
using NLog;

namespace AsylumLauncher.Data.Controls
{
    internal class InputReader
    {
        private readonly string[] UserInputLines;
        private static readonly string[] BmInputLines = { "", "" };

        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        private const int MouseSensitivityIndex = 0;
        private const int MouseSmoothingIndex = 1;

        public InputReader()
        {
            try
            {
                UserInputLines = File.ReadAllLines(Program.FileHandler.UserInputPath);
                Program.MainWindow.ControlSettingChanged = false;
                Program.MainWindow.ApplySettingsButton.Enabled = false;
                Nlog.Info("Constructor - Successfully initialized InputReader.");
            }
            catch (Exception ex)
            {
                Nlog.Error(ex, "Failed to initialize InputReader.");
                throw;
            }
        }

        public static void InitBmInputLines()
        {
            try
            {
                BmInputLines[MouseSensitivityIndex] = IniHandler.BmInputData["Engine.PlayerInput"]["MouseSensitivity"];
                BmInputLines[MouseSmoothingIndex] = IniHandler.BmInputData["Engine.PlayerInput"]["bEnableMouseSmoothing"];
            }
            catch (Exception ex)
            {
                Nlog.Error(ex, "Failed to initialize BmInputLines.");
                throw;
            }
        }

        public void InitControls()
        {
            try
            {
                SetButtons();
                SetTrackbars();
                SetMouseSettings();
                UpdateButtonColors();
            }
            catch (Exception ex)
            {
                Nlog.Error(ex, "Failed to initialize controls.");
                throw;
            }
        }

        private void SetButtons()
        {
            var buttonMappings = new (Button button, int lineIndex)[]
            {
                (Program.MainWindow.FwButton1, 5),
                (Program.MainWindow.BwButton1, 6),
                (Program.MainWindow.LeftButton1, 7),
                (Program.MainWindow.RightButton1, 8),
                (Program.MainWindow.RGUButton1, 9),
                (Program.MainWindow.CrouchButton1, 10),
                (Program.MainWindow.ZoomButton1, 11),
                (Program.MainWindow.GrappleButton1, 12),
                (Program.MainWindow.ToggleCrouchButton1, 13),
                (Program.MainWindow.DetectiveModeButton1, 18),
                (Program.MainWindow.UseGadgetStrikeButton1, 19),
                (Program.MainWindow.CTDownButton1, 17),
                (Program.MainWindow.ACTButton1, 22),
                (Program.MainWindow.GadgetSecButton1, 25),
                (Program.MainWindow.PrevGadgetButton1, 14),
                (Program.MainWindow.NextGadgetButton1, 15),
                (Program.MainWindow.CapeStunButton, 50),
                (Program.MainWindow.SpeedRunButton, 59),
                (Program.MainWindow.DebugMenuButton, 60),
                (Program.MainWindow.OpenConsoleButton, 53),
                (Program.MainWindow.ToggleHudButton, 54),
                (Program.MainWindow.ResetFoVButton, 55),
                (Program.MainWindow.CustomFoV1Button, 56),
                (Program.MainWindow.CustomFoV2Button, 57),
                (Program.MainWindow.CentreCameraButton, 58),
                (Program.MainWindow.MapButton, 16)
            };

            foreach (var (button, lineIndex) in buttonMappings)
            {
                SetButton(button, lineIndex);
            }
        }

        private void SetTrackbars()
        {
            SetTrackbar(Program.MainWindow.CustomFoV1Trackbar, Program.MainWindow.CustomFoV1ValueLabel, 56);
            SetTrackbar(Program.MainWindow.CustomFoV2Trackbar, Program.MainWindow.CustomFoV2ValueLabel, 57);
        }

        private static void SetMouseSettings()
        {
            if (int.TryParse(BmInputLines[MouseSensitivityIndex].Split('.')[0], out var sensitivity))
            {
                Program.MainWindow.MouseSensitivityTrackbar.Value = sensitivity;
                Program.MainWindow.MouseSensitivityValueLabel.Text = sensitivity.ToString();
            }

            Program.MainWindow.MouseSmoothingBox.Checked = BmInputLines[MouseSmoothingIndex].Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static void UpdateButtonColors()
        {
            foreach (var keyButton in Program.InputHandler.ButtonList)
            {
                keyButton.ForeColor = keyButton.Text.Contains("Unbound") ? Color.RoyalBlue : Color.Black;
            }
        }

        private void SetButton(Button button, int lineIndex)
        {
            button.Text = TrimLine(UserInputLines[lineIndex]);
            Program.InputHandler.ButtonList.Add(button);
        }

        private void SetTrackbar(TrackBar trackbar, Label label, int lineIndex)
        {
            var trackbarValue = UserInputLines[lineIndex].Split(',')[1].Split('"')[1];
            if (int.TryParse(trackbarValue, out var value))
            {
                trackbar.Value = value;
                label.Text = value.ToString();
            }
        }

        private static string TrimLine(string line)
        {
            var newLine = line.Length > 17 ? line[17..] : line;

            if (line.Contains("Shift=true") && !line.Contains("bIgnoreShift=true"))
            {
                return "Shift + " + ConvertLine(newLine.Split('"')[0]);
            }
            if (line.Contains("Control=true"))
            {
                return "Ctrl + " + ConvertLine(newLine.Split('"')[0]);
            }
            if (line.Contains("Alt=true"))
            {
                return "Alt + " + ConvertLine(newLine.Split('"')[0]);
            }
            return ConvertLine(newLine.Split('"')[0]);
        }

        private static string ConvertLine(string input)
        {
            for (var i = 0; i < Program.InputHandler.LinesConfigStyle.Length; i++)
            {
                if (input == Program.InputHandler.LinesConfigStyle[i])
                {
                    return Program.InputHandler.LinesHumanReadable[i];
                }
            }
            return input;
        }
    }
}
