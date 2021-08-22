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
        return !Rank.FromName(rank).Tools.Any(c=>c.flag_or_command.ToLower()==cmd.ToLower());
    }

    public override void Set( string r, string cmd, int setting ){
        var rank = Rank.FromName(r);
        rank.Tools.RemoveAll(c=>c.flag_or_command.ToLower() == cmd.ToLower());
        if(setting != 0)
            rank.Tools.Add(new Rank.Permission{
                flag_or_command = cmd,
                access = setting == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
            });
        Rank.SetRankHasTool(r, cmd, setting);
    }
}