using System.Net;
using Newtonsoft.Json;

namespace Senter.Communication;

public static class Communicator {
    public static List<ServerVersion>? GetVersions(int appId) {
        string url = "list-versions.php?app=" + appId;
        string json = Request(url);

        if (string.IsNullOrWhiteSpace(json)) throw new APIError("Invalid server response", url);

        List<ServerVersion>? versions;

        try {
            versions = JsonConvert.DeserializeObject<List<ServerVersion>>(json);
        } catch (JsonException ex) {
            throw new APIError("Errors parsing server response; " + ex.Message, url, json);
        }


        return versions;
    }

    public static List<ServerApp> GetApps() {
        string json = Request("list-apps.php");
        if (string.IsNullOrWhiteSpace(json)) throw new APIError("Invalid server response", "list-apps.php");

        List<ServerApp>? apps;
        try {
            apps = JsonConvert.DeserializeObject<List<ServerApp>>(json);
        } catch (JsonException ex) {
            throw new APIError("Errors parsing server response; " + ex.Message, "list-apps.php", json);
        }

        if (apps == null) throw new APIError("Invalid server response", "list-apps.php", json); // should never happen - the server ought to provide ANY valid json.
        return apps;
    }

    private static string Request(string resource) {
        using var request = new WebClient();
        var wr = request.DownloadString(SenterSettings.host + "/" + resource);
        return wr;
    }
}