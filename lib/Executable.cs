using System.Diagnostics;
using bajtix.Utils;

namespace Senter.Communication;

[Serializable]
public class Executable {
    public string workingDir, execute;
    [NonSerialized] public ClientApp app;
    [NonSerialized] public ClientVersion version;
    public Platform platform;

    public Executable(string path, Platform platform, ClientApp app, ClientVersion version) {

        this.workingDir = path;
        this.platform = platform;
        this.version = version;
        this.app = app;

    }

    public string FindExecutable() {
        var pkgini = Path.Combine(workingDir, "package.ini");
        if (File.Exists(pkgini)) {
            string s = new IniFile(File.ReadAllText(pkgini)).Get("start").Replace("\\n", "").Replace("\n", ""); // replace for windows. don't know what causes this issue
            execute = Path.Combine(workingDir, s);
        } else {
            var win_execs = Directory.GetFiles(workingDir, "*.exe"); // attempts to find an exec
            if (win_execs.Length > 0)
                execute = win_execs.First(w => !w.ToLower().Contains("crashhandler"));
            else
                execute = "";
        }

        return execute;
    }

    public void Execute() {
        if (string.IsNullOrWhiteSpace(execute)) {
            // TODO: thorw correct exception,
            Console.WriteLine("incorrect exec");
            return;
        }


        var currentPlatform = Management.GetCurrentPlatform();
        if (currentPlatform == Platform.mac ||
         currentPlatform == Platform.lin) SetExecPermission(execute);

        var pinfo = new ProcessStartInfo(execute)
        {
            WorkingDirectory = Path.GetFullPath(workingDir),
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false
        };
        ThreadStart ts = new ThreadStart(() => Process.Start(pinfo));
        new Thread(ts).Start();
        //Process.Start(pinfo);
    }

    public void Remove() {
        Directory.Delete(workingDir, true);
        Management.MarkUninstalled(this);
    }

    public void SetExecPermission(string file) {
        string abs = Path.GetFullPath(file);
        Process.Start($"chmod", $"+x \"{abs}\"");
    }

    public override string ToString() {
        return $"{app.name} ({version.version}) [{platform}]";
    }
}