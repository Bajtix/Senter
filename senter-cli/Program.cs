using System.Collections.Generic;
using System.Text.RegularExpressions;
using Senter.Communication;
using Console = Colorful.Console;

Platform current = Management.GetCurrentPlatform();

Utils.ObtainVersionInfo();

Management.LoadSettings();
Management.LoadLocalDB();


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


CommandManager.RegisterCommands(typeof(Commands));

Utils.DrawStartGraphic();



void Auto() {

    string script = File.ReadAllText(args[0]);
    foreach (var instruction in script.Split(Environment.NewLine)) {
        string t = instruction.Trim();
        Console.WriteLine(t);
        Exec(t);
        while (Utils.consoleBlocked) ;
    };

}

async Task Input() {

    while (true) {
        if (Utils.consoleBlocked) {
            await Task.Delay(50);
            continue;
        };
        Console.Write("せ> ");
        string input = Console.ReadLine();
        Exec(input);
    }

}

static void Exec(string input) {
    if (string.IsNullOrWhiteSpace(input)) return;
    var parts = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();
    for (int i = 0; i < parts.Count(); i++) parts[i] = parts[i].Replace("\"", "");
    if (!CommandManager.commands.ContainsKey(parts[0])) {
        Commands.CommandDoesNotExist("Command does not exist!", parts[0]);
        return;
    }

    CommandManager.RunCommand(parts[0], parts.Skip(1).ToArray());
}

if (args.Length >= 1) {
    Auto();
} else await Input();