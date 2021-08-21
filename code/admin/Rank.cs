using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public partial class Rank{
    public string Name {get; set;} = "invalid_rank";
    public string Inherit {get; set;} = null;
    public float Authority {get; set;} = 0.0f;
    public string NameColor {get; set;} = "#FFFF00";
    public List<Permission> Commands {get; set;} = new();
    public List<Permission> Flags {get; set;} = new();
    public List<Permission> AuthorityOver {get; set;} = new();

    public static Rank FromName(string name){
        return AdminCore.ranks.FirstOrDefault(r => r.Name.ToLower() == name.ToLower());
    }

    public Rank GetParent(){
        return AdminCore.ranks.FirstOrDefault(x => x.Name.ToLower() == (Inherit??"").ToLower());
    }
    public bool HasCommand(string name){
        var permission = Commands.Where(c=>c.flag_or_command.ToLower() == name.ToLower()).FirstOrDefault();
        if(permission is not null)return permission.access==Permission.Access.Allow;
        if(HasFlag("allCommands"))return true;
        return GetParent()?.HasCommand(name)??false;
    }
    public bool HasFlag(string name){
        var permission = Flags.Where(c=>c.flag_or_command.ToLower() == name.ToLower()).FirstOrDefault();
        if(permission is not null)return permission.access==Permission.Access.Allow;
        return GetParent()?.HasFlag(name)??false;
    }
    public bool CanTouch(string rankTarget){
        var permission = AuthorityOver.Where(c=>c.flag_or_command.ToLower() == rankTarget.ToLower()).FirstOrDefault();
        if(permission is not null)return permission.access==Permission.Access.Allow;
        if(HasFlag("allAuthority"))return true;
        return GetParent()?.CanTouch(rankTarget)??false;
    }
    public bool WouldLoop(string target){
        if(Name.ToLower() == target.ToLower())return true;
        var curParent = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower()==target.ToLower());
        while(curParent is not null && curParent!=this){
            target = curParent.Inherit;
            if(target is null){
                curParent = null;
                break;
            }
            curParent = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower()==target.ToLower());
        }
        return curParent is not null;
    }

    [ServerCmd]
    public static void SetRankName(string target, string newName){
        Assert.NotNull(ConsoleSystem.Caller);
        if(!ConsoleSystem.Caller.HasFlag("editRanks")){
            Log.Info($"{ConsoleSystem.Caller.Name} tried to run SetRankName ${target} ${newName}, but was not allowed!");
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You aren't authorized to do that!", "debug/particleerror.vtex");
            return;
        }
        if(Rank.FromName(newName) is not null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Rank names must be unique!", "debug/particleerror.vtex");
            return;
        }
        if(newName.Length == 0){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Rank names must not be empty!", "debug/particleerror.vtex");
            return;
        }
        var targetRank = Rank.FromName(target);
        if(targetRank is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Could not find rank to edit!", "debug/particleerror.vtex");
            return;
        }
        foreach(var adm in AdminCore.admins.Where(c=>c.rankkey.ToLower() == target.ToLower())){
            adm.rankkey = newName;
        }
        targetRank.Name = newName;
        foreach(var rnk in AdminCore.ranks){
            if((rnk.Inherit??"").ToLower() == target.ToLower())
                rnk.Inherit = newName;
        }

        AdminCore.ReinformClients();
        AdminCore.SaveData();
    }

    [ServerCmd]
    public static void SetRankInheritor(string target, string newInheritor){
        Assert.NotNull(ConsoleSystem.Caller);
        if(!ConsoleSystem.Caller.HasFlag("editRanks")){
            Log.Info($"{ConsoleSystem.Caller.Name} tried to run SetRankInheritor ${target} ${newInheritor}, but was not allowed!");
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You aren't authorized to do that!", "debug/particleerror.vtex");
            return;
        }
        var targetRank = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower()==target.ToLower());
        if(targetRank is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Could not find rank to edit!", "debug/particleerror.vtex");
            return;
        }
        if(targetRank.WouldLoop(newInheritor)){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Can't set inheritor without creating a loop. Change the other one first if you really need to.", "debug/particleerror.vtex");
            return;
        }
        targetRank.Inherit = newInheritor;
        AdminCore.ReinformClients();
        AdminCore.SaveData();
    }

    [ServerCmd]
    public static void SetRankColor(string target, string newColor){
        Assert.NotNull(ConsoleSystem.Caller);
        if(!ConsoleSystem.Caller.HasFlag("editRanks")){
            Log.Info($"{ConsoleSystem.Caller.Name} tried to run SetRankColor ${target} ${newColor}, but was not allowed!");
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You aren't authorized to do that!", "debug/particleerror.vtex");
            return;
        }
        var targetRank = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower()==target.ToLower());
        if(targetRank is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Could not find rank to edit!", "debug/particleerror.vtex");
            return;
        }
        var newCol = Color.Parse(newColor);
        if(newCol is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Tried to set to an invalid colour.", "debug/particleerror.vtex");
            return;
        }
        targetRank.NameColor = newColor;
        AdminCore.ReinformClients();
        AdminCore.SaveData();
    }

    [ServerCmd]
    public static void SetRankAuthority(string target, int newAuthority){
        Assert.NotNull(ConsoleSystem.Caller);
        if(!ConsoleSystem.Caller.HasFlag("editRanks")){
            Log.Info($"{ConsoleSystem.Caller.Name} tried to run SetRankAuthority ${target} ${newAuthority}, but was not allowed!");
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You aren't authorized to do that!", "debug/particleerror.vtex");
            return;
        }
        var targetRank = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower()==target.ToLower());
        if(targetRank is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Could not find rank to edit!", "debug/particleerror.vtex");
            return;
        }
        newAuthority = newAuthority.Clamp(0, 999);
        targetRank.Authority = newAuthority;
        AdminCore.ReinformClients();
        AdminCore.SaveData();
    }

    [ServerCmd]
    public static void SetRankHasFlag(string target, string flagTarget, int setting){
        Assert.NotNull(ConsoleSystem.Caller);
        if(!ConsoleSystem.Caller.HasFlag("editRanks")){
            Log.Info($"{ConsoleSystem.Caller.Name} tried to run SetRankHasFlag ${target} ${flagTarget} ${setting}, but was not allowed!");
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You aren't authorized to do that!", "debug/particleerror.vtex");
            return;
        }
        var targetRank = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower()==target.ToLower());
        if(targetRank is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Could not find rank to edit!", "debug/particleerror.vtex");
            return;
        }
        if(!AdminCore.AllFlags().Contains(flagTarget)){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Flag not found!", "debug/particleerror.vtex");
            return;
        }
        if(setting < -1 || setting > 1){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Invalid state! (-1: Disallow, 0: Inherit, 1: Allow)", "debug/particleerror.vtex");
            return;
        }
            targetRank.Flags.RemoveAll(c=>c.flag_or_command.ToLower() == flagTarget.ToLower());
        if(setting != 0)
            targetRank.Flags.Add(new Permission{
                flag_or_command = flagTarget,
                access = setting == -1 ? Rank.Permission.Access.Deny : Rank.Permission.Access.Allow
            });
        AdminCore.ReinformClients();
        AdminCore.SaveData();
    }

    [ServerCmd]
    public static void SetRankHasCommand(string target, string commandTarget, int setting){
        Assert.NotNull(ConsoleSystem.Caller);
        if(!ConsoleSystem.Caller.HasFlag("editRanks")){
            Log.Info($"{ConsoleSystem.Caller.Name} tried to run SetRankHasCommand ${target} ${commandTarget} ${setting}, but was not allowed!");
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You aren't authorized to do that!", "debug/particleerror.vtex");
            return;
        }
        var targetRank = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower()==target.ToLower());
        if(targetRank is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Could not find rank to edit!", "debug/particleerror.vtex");
            return;
        }
        if(!Command.commands.ContainsKey(commandTarget.ToLower())){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Command not found!", "debug/particleerror.vtex");
            return;
        }
        if(setting < -1 || setting > 1){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Invalid state! (-1: Disallow, 0: Inherit, 1: Allow)", "debug/particleerror.vtex");
            return;
        }
        targetRank.Commands.RemoveAll(c=>c.flag_or_command.ToLower() == commandTarget.ToLower());
        if(setting != 0)
            targetRank.Commands.Add(new Permission{
                flag_or_command = commandTarget.ToLower(),
                access = setting == -1 ? Rank.Permission.Access.Deny : Rank.Permission.Access.Allow
            });
        AdminCore.ReinformClients();
        AdminCore.SaveData();
    }

    [ServerCmd]
    public static void SetRankHasAuthority(string target, string authorityTarget, int setting){
        Assert.NotNull(ConsoleSystem.Caller);
        if(!ConsoleSystem.Caller.HasFlag("editRanks")){
            Log.Info($"{ConsoleSystem.Caller.Name} tried to run SetRankHasAuthority ${target} ${authorityTarget} ${setting}, but was not allowed!");
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You aren't authorized to do that!", "debug/particleerror.vtex");
            return;
        }
        var targetRank = AdminCore.ranks.FirstOrDefault(c=>c.Name.ToLower()==target.ToLower());
        if(targetRank is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Could not find rank to edit!", "debug/particleerror.vtex");
            return;
        }
        if(Rank.FromName(authorityTarget) is null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Rank not found!", "debug/particleerror.vtex");
            return;
        }
        if(setting < -1 || setting > 1){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", $"Invalid state! (-1: Disallow, 0: Inherit, 1: Allow)", "debug/particleerror.vtex");
            return;
        }
            targetRank.AuthorityOver.RemoveAll(c=>c.flag_or_command.ToLower() == authorityTarget.ToLower());
        if(setting != 0)
            targetRank.AuthorityOver.Add(new Permission{
                flag_or_command = authorityTarget,
                access = setting == -1 ? Rank.Permission.Access.Deny : Rank.Permission.Access.Allow
            });
        AdminCore.ReinformClients();
        AdminCore.SaveData();
    }

    [ServerCmd]
    public static void MakeNewRank(){
        Assert.NotNull(ConsoleSystem.Caller);
        if(!ConsoleSystem.Caller.HasFlag("editRanks")){
            Log.Info($"{ConsoleSystem.Caller.Name} tried to run MakeNewRank, but was not allowed!");
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You aren't authorized to do that!", "debug/particleerror.vtex");
            return;
        }
        if(Rank.FromName("New Rank") is not null){
            TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "A new rank already exists! Edit that first!", "debug/particleerror.vtex");
            return;
        }
        AdminCore.ranks.Add(new Rank{
            Name = "New Rank"
        });
        AdminCore.ReinformClients();
        AdminCore.SaveData();
    }

    public class Permission {
        public Access access {get; set;} = Access.Allow;
        public string flag_or_command {get; set;} = "";
        
        public enum Access {
            Deny,
            Allow
        }
    }
}