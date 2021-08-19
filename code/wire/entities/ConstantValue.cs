using System.Xml.Schema;
using Sandbox;
using System;
using System.Collections.Generic;

[Library( "ent_constantvalue", Title = "Constant Value", Spawnable = true, Group = "Wire" )]
partial class ConstantValue : Prop, IWireEntity
{
    [Net]
	public double value { get; set; } = 1.0d;
	List<WireVal> values;
	public List<WireVal> Values() {
		if(values is not null) return values;
		values = new();
		values.Add(new WireValNormal("value", "Value", WireVal.Direction.Output, ()=>value, f=>value=f));
		return values;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/chippacket01.vmdl" );
		Scale = 0.5f;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		RenderColor = Color.Red;
	}

	bool IWireEntity.ShouldGlow => true;
}
