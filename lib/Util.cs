namespace Senter.Communication;

public static class Util {
    public static T GetById<T>(this List<T> apps, int id) where T : App {
        return apps.FirstOrDefault(e => e.id == id, null);
    }

    public static T GetVersionById<T>(this List<T> vers, int id) where T : Version {
        return vers.FirstOrDefault(e => e.id == id, null);
    }

    public static T GetVersionByName<T>(this List<T> vers, string id) where T : Version {
        return vers.FirstOrDefault(e => e.version == id, null);
    }

    // thanks, https://stackoverflow.com/questions/249760/how-can-i-convert-a-unix-timestamp-to-datetime-and-vice-versa
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp) {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

}