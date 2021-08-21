using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public class CommandNoclip : Command {
    public override string Name => "Noclip";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        var target = args.ElementAtOrDefault(0)??"";
        if(target.Length == 0)
            target = "^";
        
        Client c = GetTarget(target, executor);
        if(c is null)
            return false;
        
        (Game.Current as SandboxGame)?.DoPlayerNoclip(c, true);
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.GetClientOwner().ColorName()} {((c.Pawn as SandboxPlayer).DevController is NoclipController ? "enabled" : "disabled")} noclip on {(target=="^"?"themself":c.ColorName())}.");
        return true;
    }
}