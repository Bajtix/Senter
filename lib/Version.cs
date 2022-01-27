namespace Senter.Communication;

public class Version {
    public int id;
    public string version,
    changelog,
    source;

    public DateTime dateReleaseDate;


    [NonSerialized] public App parentApp;

    public Version() { }
    public Version(Version v) {
        if (v == null) return;
        this.id = v.id;
        this.version = v.version;
        this.changelog = v.changelog;
        this.source = v.source;
        this.parentApp = v.parentApp;
        this.dateReleaseDate = v.dateReleaseDate;
    }

    public override bool Equals(object obj) {
        return obj is Version version &&
               id == version.id;
    }

    public override int GetHashCode() {
        return HashCode.Combine(id);
    }
}