using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public abstract partial class Command {
    public abstract string Name {get;}
    public abstract string Category {get;}

    public abstract bool Run(Player executor, IEnumerable<string> arguments);

    public static Dictionary<string, Command> commands = new();

    [Event.Hotload]
    public static void ReloadCommands(){
        commands = new();
        SetupCommands();
    }

    public void Register(){
        commands[Name.ToLower()] = this;
    }


}