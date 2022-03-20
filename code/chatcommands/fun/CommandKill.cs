using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

public class CommandKill : Command {
    public override string Name => "Kill";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        executor.Health = 1;
        (executor as SandboxPlayer).GodMode = false;
        executor.TakeDamage(DamageInfo.Generic(9000000f));
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.Client.Name} killed themself."); //avatar:{executor.Client.PlayerId}
        return true;
    }
}