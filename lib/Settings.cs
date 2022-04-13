namespace Senter.Communication;

public static class SenterSettings {

    public static string libraryPath {
        get => settings.libraryPath;
        set => settings.libraryPath = value;
    }
    public static string tmpPath {
        get => settings.tmpPath;
        set => settings.tmpPath = value;
    }
    public static string devPath {
        get => settings.devPath;
        set => settings.devPath = value;
    }
    public static string host {
        get => settings.host;
        set => settings.host = value;
    }

    public static string devPassword {
        get => settings.devPassword;
        set => settings.devPassword = devPassword;
    }

    public static string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/senter/senter-settings.json";


    public static Settings settings = new Settings();

    public class Settings {
        public string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/senter/library/";
        public string tmpPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/senter/tmp/";
        public string host = "http://mc.bajtix.xyz:8080/senter/backend"; // configure it to your own, if you have one!
        public string devPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/senter/dev/";
        public string devPassword = "";
    }

    public static void ResetSettings() {
        settings = new Settings();
    }


}