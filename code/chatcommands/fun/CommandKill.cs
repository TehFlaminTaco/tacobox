using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

public class CommandKill : Command {
    public override string Name => "Kill";

    public override bool Run(Player executor, IEnumerable<string> args){
        executor.TakeDamage(DamageInfo.Generic(900000f));
        ChatBox.AddChatEntry(To.Everyone, "", $"⚠️ {executor.GetClientOwner().Name} killed themself."); //avatar:{executor.GetClientOwner().SteamId}
        return true;
    }
}