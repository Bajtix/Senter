namespace Senter.Communication;

[Serializable]
public class ServerApp : App {
    public List<ServerVersion> versions;

    public List<ServerVersion> GetVersions() {
        versions = Communicator.GetVersions(id);
        foreach (var v in versions) {
            v.UpdateInfo(this);
        }
        return versions;
    }

    public ServerVersion? GetLatest() {
        if (versions == null) GetVersions();
        versions.Sort();
        return versions.LastOrDefault();
    }

}
