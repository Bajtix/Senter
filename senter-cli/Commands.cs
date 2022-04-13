using System.Drawing;
using Colorful;
using Senter.Communication;
using Console = Colorful.Console;

public static class Commands {

    public static StyleSheet globalStyle = new StyleSheet(Color.White);

    public static void AddFormatters<T>(List<T> lapps) where T : App {
        globalStyle.AddStyle(@"🝚 ([^\\ ]+)\ ", Color.Aqua, (match) => match.ToUpper());
        globalStyle.AddStyle(@"lin(?=[\]\ \n\r])|win(?=[\]\ \n\r])|mac(?=[\]\ \n\r])", Color.Green, (match) => match.ToLower());

        foreach (var w in lapps) {
            globalStyle.AddStyle(w.name, Color.Gold, (match) => match);
        }
    }

    public static T AppFromString<T>(this List<T> applist, string app) where T : App {
        int appid;
        if (!int.TryParse(app, out appid)) {
            var s = applist.Find(w => w.name == app);
            if (s == null) {
                return null;
            }
            appid = s.id;
        }

        var fapp = applist.GetById(appid); // Fetched APP

        if (fapp == null) {
            return null;
        }

        return fapp;
    }

    [Command("quit", "quit senter")]
    [Command("exit", "quit senter")]
    public static void CQuit(string[] obj, Command myself) {
        Environment.Exit(0);
    }



    [Command("dev", "enter developer mode")]
    [Command("d", "enter developer mode")]
    public static void CDev(string[] obj, Command myself) {
        if (!Utils.isOnline) {
            Error(myself, "Cannot enter dev mode while offline!");
            return;
        }

        string pass;
        if (SenterSettings.devPassword != "") {
            Console.WriteLine("Dev autologin successful!", Color.Purple);
            pass = SenterSettings.devPassword;
        } else {
            Console.WriteLine("Wanna be a dev, huh?", Color.Purple);
            Console.Write("Password:");
            pass = Console.ReadLine();
        }

        if (DevPackage.AuthenticateWithServer(pass)) {
            DevPackage.LoadLocalDB();
            Commands.AddFormatters(DevPackage.devPackages);
            Console.WriteLine("You are now in developer mode!", Color.Gold);
        } else {
            Console.WriteLine("Something went wrong with the login!", Color.Red);
        }
    }

    [Command("devnew", "create new package or locate an existing one", "folder")]
    [Command("dnew", "create new package or locate an existing one", "folder")]
    public static void CDevNew(string[] obj, Command myself) {
        if (DevPackage.currentToken.IsExpired) {
            Error(myself, "Developer token expired");
            return;
        }

        string folder = "";

        if (obj.Length >= 1)
            folder = obj[0];

        if (!Directory.Exists(folder)) {
            Error(myself, "Directory is invalid");
            return;
        }

        DevPackage.CreatePackage(folder);
    }

    [Command("devupdate", "update devpackage on the server", "app")]
    [Command("dupdate", "update devpackage on the server", "app")]
    public static void CDevUpdate(string[] obj, Command myself) {
        if (DevPackage.currentToken.IsExpired) {
            Error(myself, "Developer token expired");
            return;
        }

        DevPackage.UpdateDbFromManifests();

        DevApp fapp;

        if (obj.Length >= 1) {
            fapp = DevPackage.devPackages.AppFromString(obj[0]);
            if (fapp == null) {
                Error(myself, "App not found");
                return;
            }
        } else {
            UsageError(myself);
            return;
        }

        fapp.UpdatePackage();
    }


    [Command("devupload", "upload version to server", "app")]
    [Command("dupload", "upload version to server", "app")]
    public static void CDevUpload(string[] obj, Command myself) {
        if (DevPackage.currentToken.IsExpired) {
            Error(myself, "Developer token expired");
            return;
        }

        DevPackage.UpdateDbFromManifests();

        Platform platform = Management.GetCurrentPlatform();
        string version = "latest";

        DevApp fapp;

        if (obj.Length >= 1) {
            fapp = DevPackage.devPackages.AppFromString(obj[0]);
            if (fapp == null) {
                Error(myself, "App not found");
                return;
            }
        } else {
            UsageError(myself);
            return;
        }
        if (obj.Length >= 2)
            version = obj[1];
        if (obj.Length >= 3)
            platform = Utils.ParsePlatform(obj[2]);

        DevVersion appversion;
        if (version == "latest")
            appversion = fapp.versions.Last();
        else
            appversion = fapp.versions.GetVersionByName(version);


        if (appversion == null) {
            Error(myself, "No such version");
            return;
        }

        appversion.PrepareArtifacts();
        var upload = appversion.artifacts[platform].StartUpload();

        Utils.PrintStartUpload(upload);
        upload.onUploadProgress += (a) => Utils.PrintUploadProgress(upload, a);
        upload.onUploadFinish += (a) => Utils.PrintUploaded(upload);
    }

    [Command("devremove", "remove devpackage from dev repo", "app")]
    [Command("dremove", "remove devpackage from dev repo", "app")]
    public static void CDevRemove(string[] obj, Command myself) {
        if (DevPackage.currentToken.IsExpired) {
            Error(myself, "Developer token expired");
            return;
        }
        DevApp fapp;

        if (obj.Length >= 1) {
            fapp = DevPackage.devPackages.AppFromString(obj[0]);
            if (fapp == null) {
                Error(myself, "App not found");
                return;
            }
        } else {
            UsageError(myself);
            return;
        }

        DevPackage.RemovePackage(fapp);
    }


    [Command("setting", "set setting", "[setting|reset]", "[value]")]
    [Command("set", "set setting", "[setting|reset]", "[value]")]
    public static void CSet(string[] obj, Command myself) {
        string setting = "*";
        string value = "*";

        if (obj.Length >= 1)
            setting = obj[0];

        if (obj.Length >= 2)
            value = obj[1];

        if (setting == "reset") {
            if (setting == value) {
                SenterSettings.ResetSettings();
                Management.SaveSettings();
                Console.WriteLine("Reset settings");
                setting = "*";
            } else {
                Error(myself, "To reset, use the reset as both the setting and the value");
                return;
            }

        }

        var settings = SenterSettings.settings.GetType().GetFields();


        if (setting == "*") {
            foreach (var v in settings) {
                Console.WriteLine($"{v.Name}={v.GetValue(SenterSettings.settings)}");
            }
            return;
        }

        var settingreference = settings.FirstOrDefault(w => w.Name == setting, null);
        if (settingreference == null) {
            Error(myself, "Setting does not exist");
            return;
        }

        if (value == "*") {
            Console.WriteLine($"{settingreference.Name}={settingreference.GetValue(SenterSettings.settings)}");
            return;
        }

        settingreference.SetValue(SenterSettings.settings, value);
        Management.SaveSettings();
        Console.WriteLine($"Updated {settingreference.Name}");

    }


    [Command("list", "attempts to list the data structure", "[repo=remote]", "[app=*]")]
    [Command("ls", "attempts to list the data structure", "[repo=remote]", "[app=*]")]
    public static void CListApps(string[] obj, Command myself) {
        string repo = "remote";
        string app = "*";

        if (obj.Length >= 1) repo = obj[0];
        if (obj.Length >= 2) app = obj[1];

        if (repo == "local") {
            if (app == "*") {
                Console.WriteLineStyled("Listing apps for 🝚 LOCAL ", globalStyle);
                Console.WriteLine($"{Utils.Pad("id", 3, ' ')} | {Utils.Pad("Name", 24, ' ')} | {Utils.Pad("Description", 32, ' ')} | {Utils.Pad("Latest", 10, ' ')} | URL");
                foreach (var a in Management.local) {
                    Console.WriteLineStyled($"{a.id:000} | {Utils.Pad(a.name, 24, ' ')} | {Utils.Pad(a.description, 32, ' ')} | {Utils.Pad(a.GetLatest().version, 10, ' ')} | {a.link}", globalStyle);
                }
            } else {
                var fapp = Management.local.AppFromString(app);
                if (fapp == null) {
                    Error(myself, "App not found");
                    return;
                }

                Console.WriteLineStyled($"Listing versions for 🝚 LOCAL : {fapp.name}  ", globalStyle);
                Console.WriteLine($"id  | {Utils.Pad("Version", 10, ' ')} | {Utils.Pad("Released", 16, ' ')} | {Utils.Pad("Platforms", 16, ' ')} | Source");
                foreach (var a in fapp.versions) {
                    string platforms = Utils.WritePlatforms(a.executables.Keys);
                    Console.WriteLineStyled($"{a.id:000} | {Utils.Pad(a.version, 10, ' ')} | {Utils.Pad(a.dateReleaseDate.ToString("dd/MM/yy hh:mm"), 16, ' ')} | {Utils.Pad(platforms, 16, ' ')} | {a.source}", globalStyle);
                }
            }
        } else if (repo == "dev") {
            if (DevPackage.currentToken.IsExpired) {
                Error(myself, "Developer token expired");
                return;
            }
            if (app == "*") {
                Console.WriteLineStyled("Listing apps for 🝚 DEV ", globalStyle);
                Console.WriteLine($"{Utils.Pad("id", 3, ' ')} | {Utils.Pad("Name", 24, ' ')} | {Utils.Pad("Description", 32, ' ')} | {Utils.Pad("Latest", 10, ' ')} | {Utils.Pad("URL", 16, ' ')} | Manifest");
                foreach (var a in DevPackage.devPackages) {
                    Console.WriteLineStyled($"{a.id:000} | {Utils.Pad(a.name, 24, ' ')} | {Utils.Pad(a.description, 32, ' ')} | {Utils.Pad(a.GetLatest()?.version, 10, ' ')} | {Utils.Pad(a.link, 16, ' ')} | {a.manifestPath}", globalStyle);
                }
            } else {
                var fapp = DevPackage.devPackages.AppFromString(app);
                if (fapp == null) {
                    Error(myself, "App not found");
                    return;
                }

                if (fapp.versions == null) return;

                Console.WriteLineStyled($"Listing versions for 🝚 DEV : {fapp.name}  ", globalStyle);
                Console.WriteLine($"id  | {Utils.Pad("Version", 10, ' ')} | {Utils.Pad("Released", 16, ' ')} | {Utils.Pad("Manifest", 24, ' ')} | Source");
                foreach (var a in fapp.versions) {
                    Console.WriteLineStyled($"{a.id:000} | {Utils.Pad(a.version, 10, ' ')} | {Utils.Pad(a.dateReleaseDate.ToString("dd/MM/yy hh:mm"), 16, ' ')} | {Utils.Pad(a.manifestPath, 24, ' ')} | {a.source}", globalStyle);
                }
            }
        } else {
            if (app == "*") {
                Console.WriteLineStyled("Listing apps for 🝚 REMOTE ", globalStyle);
                Console.WriteLine($"{Utils.Pad("id", 3, ' ')} | {Utils.Pad("Name", 24, ' ')} | {Utils.Pad("Description", 32, ' ')} | {Utils.Pad("Latest", 10, ' ')} | URL");
                foreach (var a in Management.remote) {
                    Console.WriteLineStyled($"{a.id:000} | {Utils.Pad(a.name, 24, ' ')} | {Utils.Pad(a.description, 32, ' ')} | {Utils.Pad(a.GetLatest()?.version, 10, ' ')} | {a.link}", globalStyle);
                }
            } else {
                var fapp = Management.remote.AppFromString(app);
                if (fapp == null) {
                    Error(myself, "App not found");
                    return;
                }

                if (fapp.versions == null) fapp.GetVersions();

                Console.WriteLineStyled($"Listing versions for 🝚 REMOTE : {fapp.name}  ", globalStyle);
                Console.WriteLine($"id  | {Utils.Pad("Version", 10, ' ')} | {Utils.Pad("Released", 16, ' ')} | {Utils.Pad("Platforms", 16, ' ')} | Source");
                foreach (var a in fapp.versions) {
                    string platforms = Utils.WritePlatforms(a.downloadables.Keys);
                    Console.WriteLineStyled($"{a.id:000} | {Utils.Pad(a.version, 10, ' ')} | {Utils.Pad(a.dateReleaseDate.ToString("dd/MM/yy hh:mm"), 16, ' ')} | {Utils.Pad(platforms, 16, ' ')} | {a.source}", globalStyle);
                }
            }
        }


    }

    [Command("clear", "clears the screen")]
    [Command("cls", "clears the screen")]
    public static void CClearScreen(string[] obj, Command myself) {
        Console.Clear();
        Utils.DrawStartGraphic();
    }

    [Command("help", "prints help", "[command (supports regex)=.*]")]
    public static void CHelp(string[] obj, Command myself) {
        if (obj.Length == 0)
            CommandManager.PrintHelp();
        else
            CommandManager.PrintHelp(obj[0]);
    }


    [Command("refresh", "refresh apps")]
    [Command("ref", "refresh apps")]
    public static void CRefresh(string[] obj, Command myself) {
        Management.LoadSettings();
        Management.LoadLocalDB();
        DevPackage.LoadLocalDB();

        try {
            Management.Connect();
            Utils.isOnline = true;
            Console.WriteLine($"Connection to {SenterSettings.host} succesful. Running in online mode!", System.Drawing.Color.Green);
        } catch (System.Net.WebException ex) {
            Management.FakeConnect();
            Console.WriteLine($"Connection to {SenterSettings.host} failed. Running in offline mode", System.Drawing.Color.Yellow);
            Utils.isOnline = false;
        }

        Commands.AddFormatters(Management.remote);
        Commands.AddFormatters(Management.local);
        Commands.AddFormatters(DevPackage.devPackages);
    }

    [Command("remove", "remove version", "app", "[version=latest]", "[platform=current]")]
    public static void CRemove(string[] obj, Command myself) {
        Platform platform = Management.GetCurrentPlatform();
        string version = "latest";

        ClientApp fapp;

        if (obj.Length >= 1) {
            fapp = Management.local.AppFromString(obj[0]);
            if (fapp == null) {
                Error(myself, "App not found");
                return;
            }
        } else {
            UsageError(myself);
            return;
        }
        if (obj.Length >= 2)
            version = obj[1];
        if (obj.Length >= 3)
            platform = Utils.ParsePlatform(obj[2]);

        ClientVersion appversion;
        if (version == "latest")
            appversion = fapp.versions.Last();
        else
            appversion = fapp.versions.GetVersionByName(version);


        if (appversion == null) {
            Error(myself, "No such version");
            return;
        }

        if (!appversion.executables.ContainsKey(platform)) {
            Error(myself, $"This version does not contain a version for {platform}");
            return;
        }
        var executable = appversion.executables[platform];
        appversion.executables[platform].Remove();
        Console.WriteLineStyled($"Removed {executable}", globalStyle);
    }

    [Command("run", "execute version", "app", "[version=latest]", "[platform=current]")]
    [Command("start", "execute version", "app", "[version=latest]", "[platform=current]")]
    [Command("exec", "execute version", "app", "[version=latest]", "[platform=current]")]
    [Command("play", "execute version", "app", "[version=latest]", "[platform=current]")]
    public static void CRun(string[] obj, Command myself) {
        Platform platform = Management.GetCurrentPlatform();
        string version = "latest";

        ClientApp fapp;

        if (obj.Length >= 1) {
            fapp = Management.local.AppFromString(obj[0]);
            if (fapp == null) {
                Error(myself, "App not found");
                return;
            }
        } else {
            UsageError(myself);
            return;
        }
        if (obj.Length >= 2)
            version = obj[1];
        if (obj.Length >= 3)
            platform = Utils.ParsePlatform(obj[2]);

        ClientVersion appversion;
        if (version == "latest")
            appversion = fapp.versions.Last();
        else
            appversion = fapp.versions.GetVersionByName(version);


        if (appversion == null) {
            Error(myself, "No such version");
            return;
        }

        if (!appversion.executables.ContainsKey(platform)) {
            Error(myself, $"This version does not contain a release for {platform}");
            return;
        }
        Console.WriteLineStyled($"Starting {appversion.parentApp.name} ({appversion.version}) [{platform}]", globalStyle);
        appversion.executables[platform].Execute();

    }


    [Command("install", "install version", "app", "[version=latest]", "[platform=current]")]
    [Command("get", "install version", "app", "[version=latest]", "[platform=current]")]
    public static void CInstall(string[] obj, Command myself) {
        Platform platform = Management.GetCurrentPlatform();
        string version = "latest";

        ServerApp fapp;

        if (obj.Length >= 1) {
            fapp = Management.remote.AppFromString(obj[0]);
            if (fapp == null) {
                Error(myself, "App not found");
                return;
            }
        } else {
            UsageError(myself);
            return;
        }
        if (obj.Length >= 2)
            version = obj[1];
        if (obj.Length >= 3)
            platform = Utils.ParsePlatform(obj[2]);

        fapp.GetVersions();
        ServerVersion appversion;
        if (version == "latest")
            appversion = fapp.GetLatest()!;
        else
            appversion = fapp.versions.GetVersionByName(version);


        if (appversion == null) {
            Error(myself, "No such version");
            return;
        }

        if (!appversion.downloadables.ContainsKey(platform)) {
            Error(myself, $"This version does not contain a release for {platform}");
            return;
        }

        var download = appversion.downloadables[platform].StartDownload();
        Utils.PrintStartDownload(download);
        download.onDownloadProgress += (a) => Utils.PrintDownloadProgress(download, a);
        download.onInstallFinish += () => Utils.PrintInstalled(download);
    }

    [Command("update", "update version", "app", "[version=latest]", "[platform=current]")]
    [Command("up", "update version", "app", "[version=latest]", "[platform=current]")]
    public static void CUpdate(string[] obj, Command myself) {
        CRemove(obj, myself);
        CInstall(obj, myself);
    }


    public static void UsageError(Command cmd) {
        Error(cmd, $"Usage error");
    }

    public static void ArgumentError(Command cmd, string argument) {
        Error(cmd, $"Argument `{argument}` is incorrect; ");
    }

    public static void Error(Command cmd, string str) {
        Console.ForegroundColor = System.Drawing.Color.Red;
        Console.WriteLine($"{str}; \nCommand {cmd.aliases[0]} is used like this:");
        CommandManager.PrintHelp(cmd.aliases[0]);
        Console.ForegroundColor = System.Drawing.Color.White;
    }

    public static void CommandDoesNotExist(string str, string cmd) {
        Console.ForegroundColor = System.Drawing.Color.Red;
        Console.WriteLine($"{str};");
        Console.ForegroundColor = System.Drawing.Color.White;
    }


}