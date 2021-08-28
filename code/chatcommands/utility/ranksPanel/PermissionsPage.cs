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

    public override int Value( string rank, string cmd ){
        return Rank.FromName(rank).HasCommand(cmd.ToLower())?1:0;
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).Commands.Any(c=>c.name.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.Commands.RemoveAll(c=>c.name.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.Commands.Add(new Rank.Permission{
                name = cmd,
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

    public override int Value( string rank, string cmd ){
        return Rank.FromName(rank).HasFlag(cmd.ToLower())?1:0;
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).Flags.Any(c=>c.name.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.Flags.RemoveAll(c=>c.name.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.Flags.Add(new Rank.Permission{
                name = cmd,
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

    public override int Value( string rank, string cmd ){
        return Rank.FromName(rank).CanTouch(cmd.ToLower())?1:0;
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).AuthorityOver.Any(c=>c.name.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.AuthorityOver.RemoveAll(c=>c.name.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.AuthorityOver.Add(new Rank.Permission{
                name = cmd,
                access = setting == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
            });
        Rank.SetRankHasAuthority(r, cmd, setting);
    }
    public override void Make(){
        foreach(var command in AdminCore.ranks.OrderBy(c=>-c.Authority))
            Add(command.Name);
    }
}