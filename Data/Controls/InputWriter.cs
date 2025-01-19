using AsylumLauncher.Data.Display;
using AsylumLauncher.Properties;
using NLog;
using System.Text;
using System.Text.RegularExpressions;

namespace AsylumLauncher.Data.Controls
{
    internal class InputWriter
    {
        private readonly string[] UserInputLines;
        private readonly string[] BmInputLines = { "", "" };
        private bool CustomBinds = false;

        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        public InputWriter()
        {
            try
            {
                UserInputLines = File.ReadAllLines(Program.FileHandler.UserInputPath);
                BmInputLines[0] = IniHandler.BmInputData["Engine.PlayerInput"]["MouseSensitivity"];
                BmInputLines[1] = IniHandler.BmInputData["Engine.PlayerInput"]["bEnableMouseSmoothing"];
                Nlog.Info("Constructor - Successfully initialized InputWriter.");
            }
            catch (Exception ex)
            {
                Nlog.Error(ex, "Constructor - Failed to initialize InputWriter.");
                throw;
            }
        }

        public void WriteAll()
        {
            try
            {
                WriteControls();
                WriteBmInput();
                Nlog.Info("WriteAll - Successfully wrote settings to 'BmInput.ini' and 'UserInput.ini'.");
            }
            catch (Exception ex)
            {
                Nlog.Error(ex, "WriteAll - Failed to write settings.");
                throw;
            }
        }

        public void WriteControls()
        {
            try
            {
                UpdateUserInputLines();
                WriteToFile(Program.FileHandler.UserInputPath, UserInputLines);
                UpdateButtonColors();
            }
            catch (Exception ex)
            {
                Nlog.Error(ex, "WriteControls - Failed to write controls.");
                throw;
            }
        }

        private void UpdateUserInputLines()
        {
            var buttonMappings = new (string ButtonText, int LineIndex)[]
            {
                (Program.MainWindow.FwButton1.Text, 5),
                (Program.MainWindow.BwButton1.Text, 6),
                (Program.MainWindow.LeftButton1.Text, 7),
                (Program.MainWindow.RightButton1.Text, 8),
                (Program.MainWindow.RGUButton1.Text, 9),
                (Program.MainWindow.CrouchButton1.Text, 10),
                (Program.MainWindow.ZoomButton1.Text, 11),
                (Program.MainWindow.GrappleButton1.Text, 12),
                (Program.MainWindow.GrappleButton1.Text, 35),
                (Program.MainWindow.ToggleCrouchButton1.Text, 13),
                (Program.MainWindow.DetectiveModeButton1.Text, 18),
                (Program.MainWindow.UseGadgetStrikeButton1.Text, 19),
                (Program.MainWindow.CTDownButton1.Text, 17),
                (Program.MainWindow.ACTButton1.Text, 22),
                (Program.MainWindow.GadgetSecButton1.Text, 25),
                (Program.MainWindow.GadgetSecButton1.Text, 49),
                (Program.MainWindow.PrevGadgetButton1.Text, 14),
                (Program.MainWindow.PrevGadgetButton1.Text, 37),
                (Program.MainWindow.NextGadgetButton1.Text, 15),
                (Program.MainWindow.NextGadgetButton1.Text, 38),
                (Program.MainWindow.CapeStunButton.Text, 50),
                (Program.MainWindow.SpeedRunButton.Text, 59),
                (Program.MainWindow.DebugMenuButton.Text, 60),
                (Program.MainWindow.OpenConsoleButton.Text, 53),
                (Program.MainWindow.ToggleHudButton.Text, 54),
                (Program.MainWindow.ResetFoVButton.Text, 55),
                (Program.MainWindow.CustomFoV1Button.Text, 56),
                (Program.MainWindow.CustomFoV2Button.Text, 57),
                (Program.MainWindow.CentreCameraButton.Text, 58),
                (Program.MainWindow.MapButton.Text, 16)
            };

            foreach (var (buttonText, lineIndex) in buttonMappings)
            {
                UserInputLines[lineIndex] = ConvertToConfigStyle(buttonText, lineIndex);
            }

            UserInputLines[53] = SetTypeKey(UserInputLines[53]);
            UserInputLines[56] = UpdateFoVValue(UserInputLines[56], Program.MainWindow.CustomFoV1Trackbar.Value);
            UserInputLines[57] = UpdateFoVValue(UserInputLines[57], Program.MainWindow.CustomFoV2Trackbar.Value);
        }

        private void WriteToFile(string filePath, string[] lines)
        {
            using StreamWriter file = new(filePath);
            foreach (string line in lines)
            {
                file.WriteLine(line);
            }
        }

        private void UpdateButtonColors()
        {
            foreach (Button keyButton in Program.InputHandler.ButtonList)
            {
                keyButton.ForeColor = keyButton.Text.Contains("Unbound") ? Color.RoyalBlue : Color.Black;
            }
        }

        public void WriteBmInput()
        {
            try
            {
                Program.FileHandler.BmInput.IsReadOnly = false;
                File.Delete(Program.FileHandler.BmInputPath);
                Program.FileHandler.CreateConfigFile(Program.FileHandler.BmInputPath, Resources.BmInput);

                // Mouse Sensitivity
                BmInputLines[0] = Program.MainWindow.MouseSensitivityValueLabel.Text + ".0";

                // Mouse Smoothing
                BmInputLines[1] = Program.MainWindow.MouseSmoothingBox.Checked ? "true" : "false";

                List<string> BmInputFileLines = new(File.ReadAllLines(Program.FileHandler.BmInputPath));

                BmInputFileLines[5] = "MouseSensitivity=" + BmInputLines[0];
                BmInputFileLines[7] = "bEnableMouseSmoothing=" + BmInputLines[1];

                using StreamWriter BmInputFile = new(Program.FileHandler.BmInputPath);
                for (int i = 0; i < BmInputFileLines.Count; i++)
                {
                    if (i == 209)
                    {
                        InsertCustomBinds(BmInputFile, ref i, BmInputFileLines);
                    }
                    BmInputFile.WriteLine(BmInputFileLines[i]);
                }

                Program.FileHandler.BmInput.IsReadOnly = true;
                CustomBinds = false;
            }
            catch (Exception ex)
            {
                Nlog.Error(ex, "WriteBmInput - Failed to write BmInput.");
                throw;
            }
        }

        private void InsertCustomBinds(StreamWriter BmInputFile, ref int i, List<string> BmInputFileLines)
        {
            for (int j = 5; j < UserInputLines.Length; j++)
            {
                try
                {
                    if (UserInputLines[j].Contains("; Add your own custom keybinds below this line."))
                    {
                        BmInputFile.WriteLine("; Add your own custom keybinds below this line. (Automatically carried over from UserInput.ini, DO NOT MODIFY!)");
                        CustomBinds = true;
                    }
                    else if (!UserInputLines[j].Contains(';'))
                    {
                        if (CustomBinds)
                        {
                            BmInputFile.WriteLine(UserInputLines[j][..1].Contains('.') ? UserInputLines[j][1..] : UserInputLines[j]);
                        }
                        else
                        {
                            BmInputFile.WriteLine(UserInputLines[j][1..]);
                        }
                    }
                    else
                    {
                        BmInputFile.WriteLine(UserInputLines[j]);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    BmInputFile.WriteLine(UserInputLines[j]);
                }
                BmInputFileLines.Insert(i + j - 5, UserInputLines[j]);
            }
            i = BmInputFileLines.IndexOf("[Engine.DebugCameraInput]") - 1;
        }

        private string ConvertToConfigStyle(string text, int i)
        {
            Nlog.Info("ConvertToConfigStyle - Binding {0} to UserInput on line {1}.", text, (i + 1).ToString());
            text = text.Replace(" ", "");
            string configLine = UserInputLines[i];
            string configLineTrimmed = configLine[17..];
            int count = configLineTrimmed[..configLineTrimmed.IndexOf("\"")].Length;
            try
            {
                string modifier = text[..text.IndexOf('+')];
                text = ConvertLine(text[(text.IndexOf('+') + 1)..]);
            }
            catch (ArgumentOutOfRangeException)
            {
                text = ConvertLine(text);
            }
            configLine = configLine.Remove(17, count).Insert(17, text);
            configLine = SetModifier(configLine, "None");
            return configLine;
        }

        private static string SetModifier(string input, string modifier)
        {
            if (!input.Contains("Shift=") && !input.Contains("Control=") && !input.Contains("Alt="))
            {
                return input;
            }

            TimeSpan time = new(0, 0, 0, 0, 3);

            switch (modifier)
            {
                case "None":
                    input = Regex.Replace(input, @"\bAlt=true\b", "Alt=false", RegexOptions.Compiled, time);
                    input = Regex.Replace(input, @"\bShift=true\b", "Shift=false", RegexOptions.Compiled, time);
                    input = Regex.Replace(input, @"\bControl=true\b", "Control=false", RegexOptions.Compiled, time);
                    break;
                case "Shift":
                    input = Regex.Replace(input, @"\bAlt=true\b", "Alt=false", RegexOptions.Compiled, time);
                    input = Regex.Replace(input, @"\bShift=false\b", "Shift=true", RegexOptions.Compiled, time);
                    input = Regex.Replace(input, @"\bControl=true\b", "Control=false", RegexOptions.Compiled, time);
                    break;
                case "Ctrl":
                    input = Regex.Replace(input, @"\bAlt=true\b", "Alt=false", RegexOptions.Compiled, time);
                    input = Regex.Replace(input, @"\bShift=true\b", "Shift=false", RegexOptions.Compiled, time);
                    input = Regex.Replace(input, @"\bControl=false\b", "Control=true", RegexOptions.Compiled, time);
                    break;
                case "Alt":
                    input = Regex.Replace(input, @"\bAlt=false\b", "Alt=true", RegexOptions.Compiled, time);
                    input = Regex.Replace(input, @"\bShift=true\b", "Shift=false", RegexOptions.Compiled, time);
                    input = Regex.Replace(input, @"\bControl=true\b", "Control=false", RegexOptions.Compiled, time);
                    break;
            }

            return input;
        }

        private static string SetTypeKey(string consoleLine)
        {
            var trimmedLine = consoleLine[consoleLine.IndexOf(",")..];
            trimmedLine = trimmedLine[(trimmedLine.IndexOf("\"") + 1)..];
            trimmedLine = trimmedLine[..trimmedLine.IndexOf("\"")];
            var typeKeyValue = consoleLine[17..];
            typeKeyValue = typeKeyValue[..typeKeyValue.IndexOf("\"")];
            var newTypeKey = "set console TypeKey " + typeKeyValue;

            TimeSpan time = new(0, 0, 0, 0, 3);

            consoleLine = Regex.Replace(consoleLine, trimmedLine, newTypeKey, RegexOptions.Compiled, time);
            return consoleLine;
        }

        private static string UpdateFoVValue(string configLine, int fovValue)
        {
            var fovSection = configLine[configLine.IndexOf(",")..];
            fovSection = fovSection[(fovSection.IndexOf("\"") + 1)..];
            fovSection = fovSection[..fovSection.IndexOf("\"")];
            var updatedValue = "fov " + fovValue;

            TimeSpan time = new(0, 0, 0, 0, 3);

            configLine = Regex.Replace(configLine, fovSection, updatedValue, RegexOptions.Compiled, time);
            return configLine;
        }

        private static string ConvertLine(string input)
        {
            for (int i = 0; i < Program.InputHandler.LinesHumanReadable.Length; i++)
            {
                if (input == Program.InputHandler.LinesHumanReadable[i].Replace(" ", ""))
                {
                    return Program.InputHandler.LinesConfigStyle[i];
                }
            }
            return input;
        }
    }
}
