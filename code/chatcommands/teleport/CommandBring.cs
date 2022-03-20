using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public class CommandBring : Command {
    public override string Name => "Bring";
    public override string Category => "Teleport";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        var target = args.ElementAtOrDefault(0)??"";
        if(target.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Need to specify target!");
            return false;
        }
        Client c = GetTarget(target, executor);
        if(c is null)
            return false;
        
        c.Pawn.Position = executor.Position + Vector3.Up * 100f;
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.Client.ColorName()} brought {c.ColorName()} to themself."); //avatar:{executor.Client.PlayerId}
        return true;
    }
}