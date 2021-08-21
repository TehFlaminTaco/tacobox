using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using System.Linq;

public class CommandSetRank : Command {
    public override string Name => "SetRank";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        var target = args.ElementAtOrDefault(0)??"";
        var rank = args.ElementAtOrDefault(1)??"";
        if(target.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Need to specify target!");
            return false;
        }
        if(rank.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Need to specify rank!");
            return false;
        }
        Client c = GetTarget(target, executor);
        if(c is null)
            return false;
        Rank r = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower().IndexOf(rank.ToLower())>-1);
        if(c is null){
            ChatBox.AddChatEntry(To.Single(executor), "white", "", $"⚠️ Rank not found!");
            return false;
        }
        AdminCore.admins.RemoveAll(x=>x.steamid == c.SteamId);
        AdminCore.admins.Add(new AdminCore.Admin{
            steamid = c.SteamId,
            rankkey = r.Name
        });
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.GetClientOwner().ColorName()} set the rank of {c.ColorName()} to {r.Name}"); //avatar:{executor.GetClientOwner().SteamId}
        return true;
    }
}