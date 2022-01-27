namespace Senter.Communication;

public class Downloadable {
    public ServerApp app;
    public Platform platform;
    public ServerVersion version;

    public string link;
    public string potentialInstallPath;

    public Downloadable(string s, Platform p, ServerVersion v) {
        this.link = s;
        this.version = v;
        this.app = (ServerApp)v.parentApp;
        this.platform = p;

    }

    public DownloadProcess StartDownload() {
        this.potentialInstallPath = Management.GetTempPath(this);
        return new DownloadProcess(this);
    }

    public override string ToString() {
        return $"{app.name} ({version.version}) [{platform}]";
    }
}

