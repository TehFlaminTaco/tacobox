using System.Collections.Generic;
using Sandbox;

[Library( "ent_debugbeacon", Title = "Debug Beacon", Spawnable = true, Group = "Wire" )]
public partial class DebugBeacon : Prop, IWireEntity {
    [Net]
    string drawText {get; set;} = "";
    public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/sodacan01.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
        this.RenderColor = Color.Red;
    }

    List<WireVal> values;

	public string DrawText { get => drawText; set => drawText = value; }

	public List<WireVal> Values()
	{
		if(values is not null)return values;
        values = new();
        values.Add(new WireValString("Text", "Text", WireVal.Direction.Input, ()=>DrawText, s=>DrawText=s));
        return values;
	}

    [Event.Frame]
	public void OnFrame()
	{
        DebugOverlay.Text( Position, DrawText );
    }

    bool IWireEntity.ShouldGlow => true;
}
