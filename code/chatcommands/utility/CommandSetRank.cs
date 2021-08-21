using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using System.Linq;

public class CommandSetRank : Command {
    public override string Name => "SetRank";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args){
        var target = args.ElementAtOrDefault(0)??"";
        var rank = args.ElementAtOrDefault(1)??"";
        if(target.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "", $"⚠️ Need to specify target!");
            return false;
        }
        if(rank.Length == 0){
            ChatBox.AddChatEntry(To.Single(executor), "", $"⚠️ Need to specify rank!");
            return false;
        }
        Client c = Client.All.FirstOrDefault(c=>c.Name.ToLower().IndexOf(target.ToLower())>-1);
        if(c is null){
            ChatBox.AddChatEntry(To.Single(executor), "", $"⚠️ Player not found!");
            return false;
        }
        Rank r = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower().IndexOf(rank.ToLower())>-1);
        if(c is null){
            ChatBox.AddChatEntry(To.Single(executor), "", $"⚠️ Rank not found!");
            return false;
        }
        AdminCore.admins.RemoveAll(x=>x.steamid == c.SteamId);
        AdminCore.admins.Add(new AdminCore.Admin{
            steamid = c.SteamId,
            rankkey = r.Name
        });
        ChatBox.AddChatEntry(To.Everyone, "", $"⚠️ {executor.GetClientOwner().Name} set the rank of {c.Name} to {r.Name}"); //avatar:{executor.GetClientOwner().SteamId}
        return true;
    }
}