using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Permissions : Perms
{
	public override string Name => "Permissions";

    public Permissions(RankPanel.RankSettings parent) : base(parent) {}

	public override void MakeLists()
	{
		List1 = new CommandsList();
		List2 = new FlagsList();
        List3 = new AuthorityList();
	}
}

class CommandsList : Perms.ListType{
    public override string Name => "Commands";

    public override bool Has( string rank, string cmd ){
        return Rank.FromName(rank).HasCommand(cmd.ToLower());
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).Commands.Any(c=>c.flag_or_command.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.Commands.RemoveAll(c=>c.flag_or_command.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.Commands.Add(new Rank.Permission{
                flag_or_command = cmd,
                access = setting == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
            });
        Rank.SetRankHasCommand(r, cmd, setting);
    }
    public override void Make(){
        foreach(var group in Command.commands.Values.OrderBy(c=>c.Name).GroupBy(c=>c.Category).OrderBy(c=>c.Key)){
            list.Add.Label(group.Key, "groupHeader");
            foreach(var command in group)
                Add(command.Name);
        }
    }
}

class FlagsList : Perms.ListType{
    public override string Name => "Flags";

    public override bool Has( string rank, string cmd ){
        return Rank.FromName(rank).HasFlag(cmd.ToLower());
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).Flags.Any(c=>c.flag_or_command.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.Flags.RemoveAll(c=>c.flag_or_command.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.Flags.Add(new Rank.Permission{
                flag_or_command = cmd,
                access = setting == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
            });
        Rank.SetRankHasFlag(r, cmd, setting);
    }
    public override void Make(){
        foreach(var command in AdminCore.AllFlags())
            Add(command);
    }
}

class AuthorityList : Perms.ListType{
    public override string Name => "Authority Over";

    public override bool Has( string rank, string cmd ){
        return Rank.FromName(rank).CanTouch(cmd.ToLower());
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).AuthorityOver.Any(c=>c.flag_or_command.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.AuthorityOver.RemoveAll(c=>c.flag_or_command.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.AuthorityOver.Add(new Rank.Permission{
                flag_or_command = cmd,
                access = setting == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
            });
        Rank.SetRankHasAuthority(r, cmd, setting);
    }
    public override void Make(){
        foreach(var command in AdminCore.ranks)
            Add(command.Name);
    }
}