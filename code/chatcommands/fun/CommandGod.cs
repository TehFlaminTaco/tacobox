using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public class CommandGod : Command {
    public override string Name => "God";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        var target = args.ElementAtOrDefault(0)??"";
        if(target.Length == 0)
            target = "^";
        
        Client c = GetTarget(target, executor);
        if(c is null)
            return false;
        
        (c.Pawn as SandboxPlayer).GodMode = !(c.Pawn as SandboxPlayer).GodMode;
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.GetClientOwner().ColorName()} {((c.Pawn as SandboxPlayer).GodMode ? "enabled" : "disabled")} godmode on {(target=="^"?"themself":c.ColorName())}."); //avatar:{executor.GetClientOwner().SteamId}
        return true;
    }
}