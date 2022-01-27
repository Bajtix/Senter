namespace Senter.Communication;

public class ClientVersion : Version, IComparable<ClientVersion> {
    public Dictionary<Platform, Executable> executables = new Dictionary<Platform, Executable>();

    public ClientVersion(Version v) : base(v) {
    }

    int IComparable<ClientVersion>.CompareTo(ClientVersion? other) {
        if (other == null) return 1;
        return this.id.CompareTo(other.id);
    }

    public void UpdateExecs() {
        foreach (var w in executables) {
            w.Value.version = this;
            w.Value.app = parentApp as ClientApp;
        }
    }
}