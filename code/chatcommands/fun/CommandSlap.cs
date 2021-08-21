using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public class CommandSlap : Command {
    public override string Name => "Slap";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        var target = args.ElementAtOrDefault(0)??"";
        var damage = args.ElementAtOrDefault(1)??"10";
        if(target.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Need to specify target!");
            return false;
        }
        if(damage.Length == 0){
            damage = "10";
        }
        Client c = GetTarget(target, executor);
        if(c is null)
            return false;
        
        c.Pawn.TakeDamage(DamageInfo.Generic(damage.ToFloat()).WithForce(Vector3.Random * 30f * damage.ToFloat()));
        Sound.FromEntity("impact-bullet-flesh", c.Pawn);
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.GetClientOwner().ColorName()} slapped {c.ColorName()} for {damage.ToFloat()} damage."); //avatar:{executor.GetClientOwner().SteamId}
        return true;
    }
}