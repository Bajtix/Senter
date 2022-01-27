using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CommandAttribute : Attribute {
    public string name, description;
    public string[] parameters;

    public CommandAttribute(string name, string description, params string[] parameters) {
        this.name = name;
        this.description = description;
        this.parameters = parameters;
    }
}

public struct Command {
    public Action<string[], Command> action;
    public string description;
    public string[] parameters;
    public List<string> aliases;

    public Command(string description, string[] parameters) {
        this.description = description;
        this.aliases = new List<string>();
        this.parameters = parameters;
        this.action = (a, b) => { };
    }

    public override string ToString() {
        string allAliases = aliases.ToArray().ConcatAll(',');
        string exampleAliases = aliases.ToArray().ConcatAll('|');
        string allParams = parameters.ToArray().ConcatAll(' ');

        return $"{allAliases} - {description} # Usage: {exampleAliases} {allParams}";
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj == null) return false;
        if (!(obj is Command)) return false;
        return ((Command)obj).description == this.description; // we don't care about other stuff when comparing (with List<Command>::Contains)
    }
}

public static class CommandManager {
    public static Dictionary<string, int> commands = new Dictionary<string, int>(); // stores command references
    private static List<Command> commandList = new List<Command>(); // stores the commands

    ///<summary>Attempts to execute command</summary>
    public static void RunCommand(string c, string[] par) {
        if (par == null) par = new string[] { }; // prevent null parameters, so that no check is needed later
        var cmd = commandList[commands[c]];
        cmd.action.Invoke(par, cmd);
    }

    ///<summary>Pretty-prints help</summary>
    public static void PrintHelp(string forCommand = ".*") {
        try {
            Regex.IsMatch("", forCommand);
        } catch {
            Console.WriteLine($"Invalid argument. {forCommand} is not valid regex.");
            return;
        }
        foreach (var c in commandList) {
            if (c.aliases.Any(alias => Regex.IsMatch(alias, forCommand))) // if there's any alias for that
                Console.WriteLine(c);
        }
    }



    ///<summary>Registers all commands in Type</summary>
    public static void RegisterCommands(Type t) {
        var methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
        foreach (var method in methods) {
            var attributes = method.GetCustomAttributes<CommandAttribute>();
            if (attributes.Count() == 0) continue;

            foreach (var attribute in attributes) {
                var cmd = new Command(attribute.description, attribute.parameters);
                Action<string[], Command> exec = (s, e) => method.Invoke(null, new object[] { s, e });

                if (commandList.Contains(cmd)) {
                    commandList[commandList.IndexOf(cmd)].aliases.Add(attribute.name); // already exists, just add the alias
                } else {
                    cmd.action = exec;
                    cmd.aliases.Add(attribute.name); // doesn't exist, add alias and then add to the list
                    commandList.Add(cmd);
                }

                commands.Add(attribute.name, commandList.IndexOf(cmd)); // add the entry
            }
        }
    }
}