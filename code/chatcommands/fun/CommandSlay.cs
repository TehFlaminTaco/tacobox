using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public class CommandSlay : Command {
    public override string Name => "Slay";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        var target = args.ElementAtOrDefault(0)??"";
        if(target.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Need to specify target!");
            return false;
        }
        Client c = GetTarget(target, executor);
        if(c is null)
            return false;
        
        c.Pawn.Health = 1;
        c.Pawn.TakeDamage(DamageInfo.Generic(9000000f));
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.GetClientOwner().ColorName()} slayed {c.ColorName()}."); //avatar:{executor.GetClientOwner().SteamId}
        return true;
    }
}