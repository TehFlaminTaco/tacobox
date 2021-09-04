using System;
using System.Linq;
using Sandbox;
using Sandbox.UI.Construct;

public class SpawnPage : Perms
{
    public SpawnPage(RankPanel.RankSettings parent) : base(parent) {}
	public override string Name => "Spawn";

	public override void MakeLists()
	{
		List1 = new ToolList();
		List2 = new LimitList();
        List3 = new EntList();
	}
}

public class ToolList : Perms.ListType
{
	public override string Name => "Tools";

	public override void Make(){
		foreach ( var kv in Library.GetAllAttributes<Sandbox.Tools.BaseTool>().GroupBy(x=>x.Group) ){
            if(string.IsNullOrEmpty(kv.Key))continue;
            list.Add.Label(kv.Key, "groupHeader");
            foreach(var command in kv)
                Add(command.Name);
        }
	}

	public override int Value( string rank, string cmd ){
        return Rank.FromName(rank).HasTool(cmd.ToLower())?1:0;
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).Tools.Any(c=>c.name.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.Tools.RemoveAll(c=>c.name.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.Tools.Add(new Rank.Permission{
                name = cmd,
                access = setting == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
            });
        Rank.SetRankHasTool(r, cmd, setting);
    }
}

public class LimitList : Perms.ListType
{
	public override string Name => "Spawn Limits";
    public override bool IsInt => true;

	public override void Make(){
		foreach(var name in Enum.GetNames<PropType>()){
            Add(name);
        }
	}

	public override int Value( string rank, string cmd ){
        return Rank.FromName(rank).SpawnLimit(cmd.ToLower());
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).SpawnLimits.Any(c=>c.name.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.SpawnLimits.RemoveAll(c=>c.name.ToLower() == cmd.ToLower());
        if(setting > -2)
            rank.SpawnLimits.Add(new Rank.Permission.Int{
                name = cmd,
                amount = setting
            });
        Rank.SetRankSpawnLimit(r, cmd, setting);
    }
}

public class EntList : Perms.ListType
{
	public override string Name => "Entities";

	public override void Make(){
		foreach ( var kv in Library.GetAllAttributes<Sandbox.Entity>().Where(c=>c.Spawnable).GroupBy(x=>x.Group) ){
            list.Add.Label(kv.Key ?? "Misc", "groupHeader");
            foreach(var command in kv)
                Add(command.Name);
        }
	}

	public override int Value( string rank, string cmd ){
        return Rank.FromName(rank).CanSpawnEnt(cmd.ToLower())?1:0;
    }

    public override bool Inherited( string rank, string cmd ){
        return !Rank.FromName(rank).AllowedEnts.Any(c=>c.name.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.AllowedEnts.RemoveAll(c=>c.name.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.AllowedEnts.Add(new Rank.Permission{
                name = cmd,
                access = setting == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
            });
        Rank.SetRankHasEnt(r, cmd, setting);
    }
}