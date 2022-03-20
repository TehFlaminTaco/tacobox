using System.Collections.Generic;
using Sandbox;

[Library( "ent_lamp", Title = "Lamp", Spawnable = true )]
public partial class LampEntity : SpotLightEntity, IWireEntity, IGlowing
{
	[Net]
	public double toggled {get; set;}
	public Texture cookie {get; set;}
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/torch/torch.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	[Event.Tick]
	public void UpdateEnabled(){
		if(cookie is not null){
			this.LightCookie = cookie;
		}
		Enabled = toggled > 0.0d;
	}

	public void Remove()
	{
		Delete();
	}

	public List<WireVal> values;
	public List<WireVal> Values()
	{
		if(values is not null) return values;
		values = new();
		values.Add(new WireValNormal("toggled", "Enabled", WireVal.Direction.Input, ()=>toggled, f=>toggled=(float)f));
		values.Add(new WireValTexture("cookie", "Cookie", WireVal.Direction.Input, ()=>cookie, f=>cookie=f));
		return values;
	}
}
