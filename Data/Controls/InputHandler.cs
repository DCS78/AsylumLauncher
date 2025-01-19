using AsylumLauncher.Properties;
using NLog;
using System.Linq;

namespace AsylumLauncher.Data.Controls
{
    internal class InputHandler
    {
        public string UserInputFile { get; private set; }

        public string BmInputFile { get; private set; }

        public string[] LinesConfigStyle { get; private set; }

        public string[] LinesHumanReadable { get; private set; }

        private readonly string[] BannedKeys = { "OEM8", "OEM6", "OEM5", "LWIN", "RWIN", "OEM7", "SCROLL", "OEM1", "OEMTILDE", "OEM7", "NUMLOCK", "MULTIPLY", "DIVIDE", "SUBTRACT", "ADD", "DECIMAL", "PAUSE", "CLEAR" };

        public List<Button> ButtonList { get; } = new();

        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        public InputHandler()
        {
            UserInputFile = Program.FileHandler.UserInputPath;
            BmInputFile = Program.FileHandler.BmInputPath;
            LinesConfigStyle = FillConfigStyle();
            LinesHumanReadable = FillHumanReadable();
            Nlog.Info("Constructor - Successfully initialized InputHandler.");
        }

        private static string[] FillConfigStyle()
        {
            return new[]
            {
                "LeftMouseButton",
                "RightMouseButton",
                "MouseScrollUp",
                "MouseScrollDown",
                "LeftControl",
                "MiddleMouseButton",
                "ThumbMouseButton",
                "ThumbMouseButton2",
                "SpaceBar",
                "CapsLock",
                "Backslash",
                "RightAlt",
                "Underscore",
                "Equals",
                "LeftBracket",
                "RightBracket",
                "Semicolon",
                "Comma",
                "Period",
                "Slash",
                "PageUp",
                "PageDown",
                "Divide",
                "Multiply",
                "NumpadZero",
                "NumpadOne",
                "NumpadTwo",
                "NumpadThree",
                "NumpadFour",
                "NumpadFive",
                "NumpadSix",
                "NumpadSeven",
                "NumpadEight",
                "NumpadNine",
                "Add",
                "Decimal",
                "Zero",
                "One",
                "Two",
                "Three",
                "Four",
                "Five",
                "Six",
                "Seven",
                "Eight",
                "Nine",
                "Underscore",
                "TAB",
                "LeftShift",
                "LeftAlt"
            };
        }

        private static string[] FillHumanReadable()
        {
            return new[]
            {
                "Left Mouse",
                "Right Mouse",
                "Mousewheel Up",
                "Mousewheel Down",
                "Ctrl",
                "Middle Mouse",
                "Mouse Thumb 1",
                "Mouse Thumb 2",
                "Space",
                "Caps",
                "\\",
                "Right Alt",
                "_",
                "=",
                "[",
                "]",
                ";",
                ",",
                ".",
                "/",
                "Page Up",
                "Page Down",
                "Num /",
                "Num *",
                "Num 0",
                "Num 1",
                "Num 2",
                "Num 3",
                "Num 4",
                "Num 5",
                "Num 6",
                "Num 7",
                "Num 8",
                "Num 9",
                "Num +",
                "Num .",
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "-",
                "Tab",
                "Shift",
                "Alt"
            };
        }

        public void SetButton(Button bt, string text)
        {
            string txtName = text.Contains('+') ? text[(text.IndexOf("+") + 1)..].Trim() : text;

            foreach (var keyButton in ButtonList.Where(kb => kb.Text == text || kb.Text.Equals(txtName, StringComparison.OrdinalIgnoreCase) || (kb.Text.Contains('+') && kb.Text[(kb.Text.IndexOf("+") + 1)..].Trim() == text)))
            {
                keyButton.Text = "Unbound";
                keyButton.ForeColor = Color.RoyalBlue;
            }

            var buttonToUpdate = ButtonList.FirstOrDefault(kb => kb.Name == bt.Name);
            if (buttonToUpdate != null)
            {
                buttonToUpdate.Text = text;
                buttonToUpdate.ForeColor = Color.Black;
            }

            if (bt.Text.Equals("Unbound"))
            {
                bt.ForeColor = Color.RoyalBlue;
            }
        }

        public bool KeyIsBanned(KeyEventArgs e)
        {
            return BannedKeys.Any(banned => e.KeyCode.ToString().Equals(banned, StringComparison.OrdinalIgnoreCase));
        }

        public void ResetControls()
        {
            Program.FileHandler.BmInput.IsReadOnly = false;
            File.Delete(UserInputFile);
            File.Delete(BmInputFile);
            Program.FileHandler.CreateConfigFile(UserInputFile, Resources.UserInput);
            Program.FileHandler.CreateConfigFile(BmInputFile, Resources.BmInput);
            foreach (var keyButton in ButtonList)
            {
                keyButton.ForeColor = Color.Black;
            }
            Program.FileHandler.BmInput.IsReadOnly = true;
            new InputReader().InitControls();
            InputReader.InitBmInputLines();
            Nlog.Info("ResetControls - Successfully reset control scheme.");
        }
    }
}
