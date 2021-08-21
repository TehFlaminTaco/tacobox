using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public abstract partial class Command {
    public abstract string Name {get;}
    public abstract string Category {get;}

    public abstract bool Run(Player executor, IEnumerable<string> arguments, bool silent);

    public static Dictionary<string, Command> commands = new();

    [Event.Hotload]
    public static void ReloadCommands(){
        commands = new();
        SetupCommands();
    }

    public void Register(){
        commands[Name.ToLower()] = this;
    }

    public static Client GetTarget(string name, Player executor, bool useTouch = true, bool informError = true){
        Client c = Client.All.FirstOrDefault(c=>c.Name.ToLower().IndexOf(name.ToLower())>-1);
        if(c is null && informError){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Player not found!");
        }
        if(useTouch && !executor.GetClientOwner().CanTouch(c)){
            if(informError) ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Cannot target {c.ColorName()}!");
            return null;
        }
        return c;
    }

}