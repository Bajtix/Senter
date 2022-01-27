using System.Drawing;
using System.Net;
using Senter.Communication;
using Console = Colorful.Console;

public static class Utils {
    public static bool consoleBlocked = false;
    public static string version = "v0.1.0b";

    public static bool isOnline;


    public static string Pad(string a, int l, char pad = '-') {
        if (a == null) a = "";

        if (a.Length > l) {
            a = a.Substring(0, l - 2);
            a += "..";
        } else if (a.Length < l) {
            while (a.Length < l) {
                a += pad;
            }
        }

        return a;
    }

    public static void ObtainVersionInfo() {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location); // obtains version
        version = fvi.FileVersion;
    }

    public static void DrawStartGraphic() {



        Console.ForegroundColor = System.Drawing.Color.White;
        Console.WriteAscii("senter");
        Console.CursorTop -= 3;
        Console.CursorLeft = 30;
        Console.Write("v" + version, Color.DarkGray);
        Console.CursorLeft = 0;
        Console.CursorTop++;
        Console.WriteLine("Developed by bajtixone \n");
        var current = Management.GetCurrentPlatform();
        var rs = new[] { current.ToString(), Environment.OSVersion.VersionString };
        Console.WriteLineFormatted("Running as [{0}] under {1}", rs, System.Drawing.Color.Green, System.Drawing.Color.White);
        Console.Title = $"SENTER-CLI [{current}]";
    }

    public static T GetByValue<T, K>(this Dictionary<T, K> a, K val) {
        Dictionary<K, T> reversed = new Dictionary<K, T>();
        foreach (var kp in a) {
            reversed.Add(kp.Value, kp.Key);
        }

        T output;
        reversed.TryGetValue(val, out output);

        return output;
    }

    public static string ConcatAll(this string[] arr, char sep) {
        string all = "";
        foreach (var s in arr) {
            all += s + sep;
        }
        if (all.Length > 0) all = all.Substring(0, all.Length - 1);

        return all;
    }

    public static Platform ParsePlatform(string s) {
        if (s == "current") return Management.GetCurrentPlatform();
        switch (s) {
            case "win":
                return Platform.win;
            case "lin":
                return Platform.lin;
            case "mac":
                return Platform.mac;
            default:
                return Platform.idk;
        }
    }

    static int progressDialogTop;
    static bool lockthr = false;


    public static void PrintProgress(DownloadProcess process, DownloadProgressChangedEventArgs obj) {
        if (lockthr) return;
        lockthr = true;
        Console.CursorTop = progressDialogTop;
        Console.CursorLeft = 1;
        string twr = $"{process.downloadable.version.version} |";
        for (int i = 0; i <= obj.ProgressPercentage / 2; i++) {
            twr += "■";
        }
        for (int i = obj.ProgressPercentage / 2; i < 50; i++) {
            twr += " ";
        }
        twr += ($"| {obj.ProgressPercentage}%");
        Console.CursorLeft = 1;
        Console.Write(twr);
        lockthr = false;
    }

    public static string WritePlatforms(IEnumerable<Platform> keys) {
        string output = "";
        foreach (var s in keys) {
            output += s.ToString() + " ";
        }
        return output;
    }

    public static void PrintInstalled(DownloadProcess process) {
        Console.CursorTop++;
        Console.CursorLeft = 0;
        Console.WriteLineStyled($"Finished installing {process.downloadable}", Commands.globalStyle);
        consoleBlocked = false;
    }

    public static void PrintStartDownload(DownloadProcess process) {
        progressDialogTop = Console.CursorTop + 1;
        Console.WriteLineStyled($"Downloading {process.downloadable}", Commands.globalStyle);
        consoleBlocked = true;
    }
}