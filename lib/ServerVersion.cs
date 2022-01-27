namespace Senter.Communication;

[Serializable]
public class ServerVersion : Version, IComparable<ServerVersion> {

    public string releasedate,
    win,
    lin,
    mac;

    [NonSerialized]
    public Dictionary<Platform, Downloadable> downloadables;


    public void UpdateInfo(ServerApp app) {
        this.parentApp = app;
        downloadables = new Dictionary<Platform, Downloadable>();
        if (!string.IsNullOrWhiteSpace(win))
            downloadables.Add(Platform.win, new Downloadable(win, Platform.win, this));
        if (!string.IsNullOrWhiteSpace(lin))
            downloadables.Add(Platform.lin, new Downloadable(lin, Platform.lin, this));
        if (!string.IsNullOrWhiteSpace(mac))
            downloadables.Add(Platform.mac, new Downloadable(mac, Platform.mac, this));

        dateReleaseDate = DateTime.Parse(releasedate);
    }

    int IComparable<ServerVersion>.CompareTo(ServerVersion other) {
        return this.id.CompareTo(other.id);
    }
}
