using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public class CommandHP : Command {
    public override string Name => "HP";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        var target = args.ElementAtOrDefault(0)??"";
        var health = args.ElementAtOrDefault(1)??"10";
        if(target.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Need to specify target!");
            return false;
        }
        if(health.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Need to specify health!");
            return false;
        }
        Client c = GetTarget(target, executor);
        if(c is null)
            return false;
        if(health.ToFloat()<1)
            health = "1";
        

        c.Pawn.Health = health.ToFloat();
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.GetClientOwner().ColorName()} set the health of {c.ColorName()} to {health.ToFloat()}."); //avatar:{executor.GetClientOwner().SteamId}
        return true;
    }
}