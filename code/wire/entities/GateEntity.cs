using System.Xml.Schema;
using Sandbox;
using System;
using System.Collections.Generic;

[Library( "ent_gate" )]
public partial class GateEntity : Prop, IWireEntity
{
    [Net]
	public double value { get; set; } = 1.0d;

	[Net]
	public string gateType {get; set;} = "maths_add"; 

	Dictionary<string, StoredWireVal> gateValues = new();
	List<WireVal> values;
	public List<WireVal> Values() {
		if(values is not null) return values;
		values = new List<WireVal>();
		foreach(var inp in getGate().Inputs){
			var stor = WireVal.FromType(inp.t, inp.key, inp.name, WireVal.Direction.Input);
			gateValues[inp.key] = stor;
			values.Add(stor.val);
		}
		foreach(var oup in getGate().Outputs){
			var stor = WireVal.FromType(oup.t, oup.key, oup.name, WireVal.Direction.Output);
			gateValues[oup.key] = stor;
			values.Add(stor.val);
		}
		return values;
	}

	public Gate getGate(){
		return Gate.gatesByKey.ContainsKey(gateType) ? Gate.gatesByKey[gateType] : null;
	}

	public T GetInput<T>(string key){
		Values(); // Ensures everything is setup.
		return (gateValues[key] as StoredWireVal<T>).data;
	}

	public void SetOutput<T>(string key, T Value){
		Values(); // Ensures everything is setup.
		(gateValues[key] as StoredWireVal<T>).data = Value;
	}

	public string Name() {
		return getGate().Name;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/chippacket01.vmdl" );
        Scale = 0.5f;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		RenderColor = Color.Red;
	}

	[Event.Tick]
	public void ProcessGate(){
		getGate().Process(this);
	}

	bool IWireEntity.ShouldGlow => true;
}
