using System.Collections.Specialized;
using System.Net;
using Newtonsoft.Json;

namespace Senter.Communication;

public static class DevPackage {
    [Serializable]
    public struct Token {
        [JsonProperty]
        private string ip, token;
        [JsonProperty]
        private long expires;

        public DateTime GetExpiryDate() {
            return Util.UnixTimeStampToDateTime(expires);
        }

        public bool IsExpired => DateTime.Now > GetExpiryDate();

        public string GetToken() {
            if (IsExpired) throw new Exception("Token expired");
            return token;
        }
    }

    public static List<DevApp> devPackages;
    public static Token currentToken;

    public static void LoadLocalDB() {
        if (File.Exists(Path.Combine(SenterSettings.devPath, "manifest.json"))) {
            string json = File.ReadAllText(Path.Combine(SenterSettings.devPath, "manifest.json"));
            devPackages = JsonConvert.DeserializeObject<List<DevApp>>(json);
            UpdateDbFromManifests();
        } else
            devPackages = new();
    }

    public static void SaveLocalDB() {
        string json = JsonConvert.SerializeObject(devPackages);
        Directory.CreateDirectory(SenterSettings.devPath);
        File.WriteAllText(Path.Combine(SenterSettings.devPath, "manifest.json"), json);
    }

    public static bool AuthenticateWithServer(string password) {
        using var wc = new WebClient();
        wc.Headers.Add("password", password);
        var reponse = wc.DownloadString(SenterSettings.host + "token.php");
        if (reponse.Contains("bad")) {
            return false; // something went south
        }

        currentToken = JsonConvert.DeserializeObject<Token>(reponse);

        return !currentToken.IsExpired;
    }

    public static bool AssertAuth() {
        return currentToken.IsExpired;
    }

    private static void CreateEmptyManifest(string path) {
        var json = JsonConvert.SerializeObject(new App());
        File.WriteAllText(path, json);
    }

    public static void CreatePackage(string directory) {
        if (devPackages == null) LoadLocalDB(); // better safe than sorry

        // step 0: find the manifest. if does not exist, throw an error
        var manifestPath = Path.Combine(directory, "package.json");
        if (!File.Exists(manifestPath)) CreateEmptyManifest(manifestPath);
        // first, we deserialize into an App type
        var application = JsonConvert.DeserializeObject<App>(File.ReadAllText(manifestPath));
        // now we need to check whether the app has an id assigned in the manifest. If this is not the case, we need to retrieve it from the server
        if (application.id == -1) {
            // get next id from the server. This also creates an empty package on the server. We also have the id now. 
            int id = RequestNewPackage();

            application.id = id;
            WriteToManifest(manifestPath, application);
        }

        AddPackage(new DevApp(application, manifestPath));

        devPackages.Last().GetVersions();

        Management.Connect(); // refresh server so no exceptions are thrown later

    }

    public static void AddPackage(DevApp applcication) {
        if (!devPackages.Contains(applcication))
            devPackages.Add(applcication);
        SaveLocalDB();
    }

    public static void WriteToManifest(string path, App application) {
        var json = JsonConvert.SerializeObject(application, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static void WriteToManifest(string path, Version version) {
        var json = JsonConvert.SerializeObject(version, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    private static int RequestNewPackage() {
        // we send a request to create a new package and retrieve the id
        var result = MakeRequest("create-app.php", new() { }, true);
        if (int.TryParse(result, out int id)) {
            return id;
        }

        throw new Exception("Exception while requesting package creation: " + result);
    }

    public static void UpdateDbFromManifests() {
        for (int i = 0; i < devPackages.Count; i++) {
            string manifest = devPackages[i].manifestPath;
            if (!File.Exists(manifest)) { devPackages.Remove(devPackages[i]); continue; }
            var appFromManifest = JsonConvert.DeserializeObject<App>(File.ReadAllText(manifest));

            if (appFromManifest.id != devPackages[i].id) {
                appFromManifest.id = devPackages[i].id;
                WriteToManifest(manifest, appFromManifest);
                Console.WriteLine($"ID cannot change after initial assignment! (app: {appFromManifest.name})");
            }

            var dapp = new DevApp(appFromManifest, manifest);
            dapp.GetVersions();
            devPackages[i] = dapp;
        }
        SaveLocalDB();
    }

    public static void RemovePackage(DevApp fapp) {
        devPackages.Remove(fapp);
        File.Delete(fapp.manifestPath);
        SaveLocalDB();
    }

    public static string MakeRequest(string uri, NameValueCollection val, bool writeToken = true) {
        using var wc = new WebClient();
        if (writeToken && !currentToken.IsExpired) wc.Headers.Add("token", currentToken.GetToken());
        return System.Text.Encoding.UTF8.GetString(wc.UploadValues(SenterSettings.host + "/" + uri, val));
    }
}