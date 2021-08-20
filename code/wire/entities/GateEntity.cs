using System.Xml.Schema;
using Sandbox;
using System;
using System.Collections.Generic;
using Sandbox.UI;
using Sandbox.UI.Construct;

[Library( "ent_gate" )]
public partial class GateEntity : Prop, IWireEntity, ITargetID, IGlowing
{

    [Net]
	public double value { get; set; } = 1.0d;

	[Net]
	public string gateType {get; set;} = "maths_add"; 

	[Net]
	public Player GateSpawner {get; set;}

	Dictionary<string, StoredWireVal> gateValues = new();
	public List<WireVal> values;
	public List<WireVal> Values() {
		if(values is not null) return values;
		values = getGate().GenerateValues(GateSpawner, this);
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

		SetModel( "models/wirebox/katlatze/chip_square.vmdl" );
        Scale = 0.5f;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	[Event.Tick]
	public void ProcessGate(){
		getGate().Process(this);
	}

	Label labelName;
	public void GenerateTargetID(Panel panel){
		labelName = panel.Add.Label("Gate");
	}

	public void TickTargetID(){
		labelName.Text = Name();
	}
}
