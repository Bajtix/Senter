namespace Senter.Communication;

public class Uploadable {
    public DevApp app;
    public Platform platform;
    public DevVersion version;
    public string folderPath;

    public Uploadable(string s, Platform p, DevVersion v) {
        this.folderPath = s;
        this.version = v;
        this.app = (DevApp)v.parentApp;
        this.platform = p;
    }

    public UploadProcess StartUpload() {
        return new UploadProcess(this);
    }

    public override string ToString() {
        return $"UPLOAD {app.name} ({version.version}) [{platform}]";
    }
}

