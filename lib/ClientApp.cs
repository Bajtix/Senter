namespace Senter.Communication;

public class ClientApp : App {
    public List<ClientVersion> versions = new List<ClientVersion>();

    public ClientApp(App other) : base(other) { }

    public ClientVersion GetLatest() {
        versions.Sort();
        return versions.Last();
    }

    public void UpdateVersions() {
        foreach (var w in versions) {
            w.parentApp = this;
            w.UpdateExecs();
        }
    }
}