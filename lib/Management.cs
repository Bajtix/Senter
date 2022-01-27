using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Senter.Communication;

public static class Management {
    public static List<ServerApp> remote;
    public static List<ClientApp> local;

    public static List<DownloadProcess> processes;

    public static bool isOflline;



    public static void FakeConnect() {
        isOflline = true;
        remote = new List<ServerApp>();
    }

    public static void SaveSettings() {
        string json = JsonConvert.SerializeObject(SenterSettings.settings);
        Directory.CreateDirectory(Path.GetDirectoryName(SenterSettings.settingsPath)!);
        File.WriteAllText(SenterSettings.settingsPath, json);
    }

    public static void LoadSettings() {
        if (!File.Exists(SenterSettings.settingsPath)) return;
        string json = File.ReadAllText(SenterSettings.settingsPath);
        SenterSettings.settings = JsonConvert.DeserializeObject<SenterSettings.Settings>(json);
    }

    public static string GetTempPath(Downloadable d) {
        return $"{Path.Combine(SenterSettings.tmpPath, d.app.name)}_{d.version.version.Replace('.', '.')}_{d.platform}.zip";
    }

    public static string GetInstallationPath(Downloadable d) {
        return Path.Combine($"{Path.Combine(SenterSettings.libraryPath, d.app.name)}", $"{d.version.version.Replace(".", "_")}_{d.platform}");
    }

    public static void Connect() {
        remote = Communicator.GetApps();
        isOflline = false;
    }

    public static Platform GetCurrentPlatform() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return Platform.lin;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Platform.win;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Platform.mac;
        else
            return Platform.idk;
    }

    public static void MarkInstalled(Downloadable d) {
        // if app is not in local repo, add it
        if (!local.Contains((App)d.app)) {
            ClientApp clientApp = new ClientApp((App)d.app);
            local.Add(clientApp);
        }

        // add the downloaded version, with the specific platform
        ClientVersion version = new ClientVersion((Version)d.version); // basically, we convert from one version to the other and copy their fields
        var app = local.GetById(d.app.id);
        if (!app.versions.Contains(version))
            app.versions.Add(version);
        version = local.GetById(d.app.id).versions.GetVersionByName(version.version);

        var executable = new Executable(GetInstallationPath(d), d.platform, app, version);

        executable.FindExecutable();

        if (version.executables.ContainsKey(d.platform)) version.executables[d.platform] = executable;
        else version.executables.Add(d.platform, executable);

        SaveLocalDB();
    }

    public static void MarkUninstalled(Executable e) {
        e.version.executables.Remove(e.platform);
        if (e.version.executables.Count == 0)
            e.app.versions.Remove(e.version);
        if (e.app.versions.Count == 0)
            Management.local.Remove(e.app);

        SaveLocalDB();
    }

    public static bool IsInstalled(Version v) {
        if (local.Contains(v.parentApp)) {
            if (local.GetById(v.parentApp.id).versions.Contains(v)) return true;
        }
        return false;
    }

    public static bool IsInstalled(App a) {
        if (local.Contains(a))
            return true;
        return false;
    }

    public static void SaveLocalDB() {
        string json = JsonConvert.SerializeObject(local);
        File.WriteAllText(Path.Combine(SenterSettings.libraryPath, "manifest.json"), json);
    }

    public static void LoadLocalDB() {
        if (File.Exists(Path.Combine(SenterSettings.libraryPath, "manifest.json"))) {
            string json = File.ReadAllText(Path.Combine(SenterSettings.libraryPath, "manifest.json"));
            local = JsonConvert.DeserializeObject<List<ClientApp>>(json);
            foreach (var a in local) a.UpdateVersions(); // after loading.
        } else
            local = new();
    }
}