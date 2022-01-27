namespace Senter.Communication;

[Serializable]
public class App {
    public int id = -1;
    public string name = "New Application",
    description = "...",
    icon = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==", // a red dot in base64
    link = "https://bajtix.xyz";

    public App() { }

    public App(App other) {
        if (other == null) return;
        this.id = other.id;
        this.name = other.name;
        this.description = other.description;
        this.icon = other.icon;
        this.link = other.link;
    }

    public override bool Equals(object? obj) { // we just care about the id while comparing.
        if (obj == null) return false;
        if (!(obj is App)) return false;

        var oap = obj as App;
        if (oap!.id == this.id) return true;
        return false;
    }

    public override int GetHashCode() {
        return HashCode.Combine(id);
    }
}