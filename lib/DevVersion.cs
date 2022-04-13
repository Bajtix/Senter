using System.IO.Compression;
using System.Net;

namespace Senter.Communication;

public class DevVersion : Version, IComparable<DevVersion> {
    [NonSerialized]
    public string manifestPath;

    public string win, lin, mac;

    public Dictionary<Platform, Uploadable> artifacts = new Dictionary<Platform, Uploadable>();

    public void SendVersion() {
        var r = DevPackage.MakeRequest("update-version.php", new()
        {
            { "id", id.ToString() },
            { "version", version },
            { "source", source },
            { "changelog", changelog },
            { "date", dateReleaseDate.ToString("yyyy-MM-dd HH:mm:ss") },
            { "app", this.parentApp.id.ToString() },
            { "vwin", win },
            { "vlin", lin },
            { "vmac", mac }
        });
        Console.WriteLine(r);
    }

    public void PrepareArtifacts() {
        if (artifacts.Count > 0) return;
        MakeArtifact(Platform.win);
        MakeArtifact(Platform.lin);
        MakeArtifact(Platform.mac);
    }

    private void MakeArtifact(Platform p) {
        var dirName = p.ToString();
        var directory = Path.Combine(Path.GetDirectoryName(manifestPath), dirName);
        artifacts.Add(p, new Uploadable(directory, p, this));
    }

    [Obsolete]
    public void UploadArtifact(Platform platform) {
        var dirName = platform.ToString();
        var directory = Path.Combine(Path.GetDirectoryName(manifestPath), dirName);
        var targetZip = Path.Combine(Path.GetDirectoryName(manifestPath), dirName + ".zip");
        if (Directory.GetFiles(directory).Length <= 0) {
            Console.WriteLine($"directory {platform} is empty");
        }
        if (File.Exists(targetZip))
            File.Delete(targetZip);
        ZipFile.CreateFromDirectory(directory, targetZip);

        using var wc = new WebClient();
        wc.Headers.Add("token", DevPackage.currentToken.GetToken());
        wc.Headers.Add("app", parentApp.id.ToString());
        wc.Headers.Add("version", version);
        wc.Headers.Add("platform", platform.ToString());
        var r = wc.UploadFile(new Uri(SenterSettings.host + "upload.php"), targetZip);
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(r));
        //TODO: error handling & async upload
    }

    int IComparable<DevVersion>.CompareTo(DevVersion other) {
        return this.id.CompareTo(other.id);
    }
}