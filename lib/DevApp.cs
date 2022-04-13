using Newtonsoft.Json;

namespace Senter.Communication;

public class DevApp : App {
    [NonSerialized]
    public List<DevVersion> versions = new List<DevVersion>();

    public string manifestPath;

    public DevApp(App other, string manifestPath) : base(other) {
        this.manifestPath = manifestPath;
    }

    public void GetVersions() {
        versions = new();

        string dir = Path.GetDirectoryName(manifestPath)!; // possible invalid path exception?
        foreach (var cd in Directory.GetDirectories(dir)) {
            AddVersionFromDirectory(cd);
        }
    }

    public void AddVersionFromDirectory(string cd) {
        string potentialManifestPath = Path.Combine(cd, "version.json");
        if (!File.Exists(potentialManifestPath)) return;
        var version = JsonConvert.DeserializeObject<DevVersion>(File.ReadAllText(potentialManifestPath));
        version.manifestPath = potentialManifestPath;

        if (Management.remote.GetById(id) == null) {
            throw new Exception($"Developer mode indexing failed, the app {name}[{id}] is not pressent on the server.");
        }

        if (version.id <= 0) {

            int vid = 1;

            if (Management.remote.GetById(id).versions == null) {
                Management.remote.GetById(id).GetVersions();
            }

            if (Management.remote.GetById(id).versions != null && Management.remote.GetById(id).versions.Count != 0) {
                vid = Management.remote.GetById(id).GetLatest().id + 1;
            }

            version.id = vid;
            version.dateReleaseDate = DateTime.Now;

            if (string.IsNullOrWhiteSpace(version.version)) {
                var dirName = new DirectoryInfo(cd).Name;
                version.version = dirName;
            }
        }



        AddVersion(version);

        DevPackage.WriteToManifest(potentialManifestPath, version);
    }

    public DevVersion? GetLatest() {
        if (versions == null) return null;
        versions.Sort();
        return versions.LastOrDefault();
    }

    public void AddVersion(DevVersion v) {
        v.parentApp = this;

        versions.Add(v);
    }


    public void UpdatePackage() {
        Management.Connect();

        string res = DevPackage.MakeRequest("update-app.php", new()
        {
            { "id", id.ToString() },
            { "name", name },
            { "description", description },
            { "link", link },
            { "icon", icon }
        });

        Console.WriteLine(res);

        GetVersions();

        foreach (var version in versions) {
            version.SendVersion();
        }
    }
}