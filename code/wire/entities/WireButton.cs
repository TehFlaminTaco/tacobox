using System.Collections.Generic;
using Sandbox;

[Library( "ent_wirebutton" )]
public partial class WireButton : Prop, IWireEntity, IGlowing, IUse
{
    public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/wirebox/katlatze/button.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
    }

	[Net]
	public double low_value { get; set; } = 0.0d;
    [Net]
	public double high_value { get; set; } = 0.0d;
    [Net]
	public double value { get; set; } = 0.0d;
    [Net]
    public bool held {get; set;} = false;
    [Net]
    public bool toggled {get; set;} = false;
    [Net]
    public bool toggleable {get; set;} = false;
	List<WireVal> values;
	public List<WireVal> Values() {
		if(values is not null) return values;
		values = new();
		values.Add(new WireValNormal("out", "Out", WireVal.Direction.Output, ()=>value, f=>value=f));
		return values;
	}

    bool wasHeld;
    [Event.Tick]
    public void UnsetToggled(){
        if(IsClient){
            SetMaterialGroup((toggleable ? toggled : held) ? 1 : 0);
        };

        if(held && !wasHeld){
            toggled = !toggled; // Does nothing if this is not toggleable.
        }

        value = (toggleable ? toggled : held) ? high_value : low_value;

        wasHeld = held;
        held = false;
    }

	public bool OnUse( Entity user ){
        held = true;
        return true;
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}
}