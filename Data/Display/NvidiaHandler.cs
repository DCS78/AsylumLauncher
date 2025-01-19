using NLog;
using NvAPIWrapper;
using NvAPIWrapper.DRS;
using NvAPIWrapper.Native.Exceptions;

namespace AsylumLauncher.Data.Display
{
    internal class NvidiaHandler
    {
        private readonly DriverSettingsProfile _prof;
        private readonly DriverSettingsSession _session;

        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        public NvidiaHandler()
        {
            try
            {
                NVIDIA.Initialize();
                Nlog.Debug("Constructor - NVIDIA API initialized.");
                _session = DriverSettingsSession.CreateAndLoad();
                _prof = GetOrCreateProfile("Batman: Arkham Asylum");
                InitialSetHbaoPlus();
                Nlog.Info("Constructor - NVIDIA profile fully processed.");
            }
            catch (NVIDIANotSupportedException e)
            {
                DisableControls();
                Nlog.Warn("Constructor - Caught NVIDIANotSupportedException: {0}", e);
            }
            catch (Exception e)
            {
                DisableControls();
                Nlog.Error("Constructor - Unexpected critical error during NVAPI initialization: {0}", e);
            }
        }

        private DriverSettingsProfile GetOrCreateProfile(string profileName)
        {
            try
            {
                return _session.FindProfileByName(profileName);
            }
            catch (NVIDIAApiException)
            {
                var profile = DriverSettingsProfile.CreateProfile(_session, profileName);
                var profApp = ProfileApplication.CreateApplication(profile, "shippingpc-bmgame.exe");
                profile = profApp.Profile;
                SetAmbientOcclusionSettings(profile, 0, 0);
                _session.Save();
                Nlog.Warn("Constructor - NVIDIA profile not found. Creating profile: {0}", profile);
                return profile;
            }
        }

        private static void DisableControls()
        {
            Program.MainWindow.RunAsAdminButton.Enabled = false;
            Program.MainWindow.hbaopluscheckbox.Enabled = false;
        }

        public void ToggleHbaoPlus(bool active)
        {
            try
            {
                if (active)
                {
                    _prof.SetSetting(2916165, 48);
                }
                SetAmbientOcclusionSettings(_prof, active ? 1 : 0, active ? 2 : 0);
                _session.Save();
                Nlog.Debug("ToggleHbaoPlus - setting AmbientOcclusionModeActive to {0}, setting AmbientOcclusionMode to {1}.",
                    _prof.GetSetting(KnownSettingId.AmbientOcclusionModeActive).CurrentValue,
                    _prof.GetSetting(KnownSettingId.AmbientOcclusionMode).CurrentValue);
            }
            catch (Exception e)
            {
                DisableControls();
                Nlog.Error("ToggleHbaoPlus - Unexpected critical error while trying to set the HBAO+ flag: {0}", e);
            }
        }

        private static void SetAmbientOcclusionSettings(DriverSettingsProfile profile, int active, int mode)
        {
            profile.SetSetting(KnownSettingId.AmbientOcclusionModeActive, (uint)active);
            profile.SetSetting(KnownSettingId.AmbientOcclusionMode, (uint)mode);
        }

        public void InitialSetHbaoPlus()
        {
            try
            {
                var aoActive = _prof.GetSetting(KnownSettingId.AmbientOcclusionModeActive).ToString();
                var aoValue = _prof.GetSetting(KnownSettingId.AmbientOcclusionMode).ToString();
                var compValue = short.Parse(_prof.GetSetting(2916165)?.CurrentValue?.ToString() ?? "0");

                if (aoActive.Contains('1') && aoValue.Contains('2') && compValue == 48)
                {
                    Program.MainWindow.hbaopluscheckbox.Checked = true;
                }
                else
                {
                    Program.MainWindow.hbaopluscheckbox.Checked = false;
                    SetAmbientOcclusionSettings(_prof, 0, 0);
                    _session.Save();
                    Nlog.Warn("InitialSetHbaoPlus - Couldn't find ambient occlusion settings. Generating settings with default(0) values now.");
                }

                Nlog.Debug("InitialSetHbaoPlus - HBAO+ is currently {0}.", (compValue == 48).ToString());
            }
            catch (Exception)
            {
                Program.MainWindow.hbaopluscheckbox.Checked = false;
                SetAmbientOcclusionSettings(_prof, 0, 0);
                _session.Save();
                Nlog.Warn("InitialSetHbaoPlus - Couldn't find ambient occlusion settings. Generating settings with default(0) values now.");
            }
        }
    }
}
