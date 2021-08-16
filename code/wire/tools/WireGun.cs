

using System.Linq;
using System;
using Sandbox;

[Library( "wiregun", Title = "Wire Gun",  Spawnable = true, Group = "Wire" )]
public partial class WireGun : Carriable {
	[ConVar.ClientData("wire_selection")]
	public static string wire_select {get; set;}
	public static int wire_selection {get; set;} = 0;

    public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
    public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );

		CollisionGroup = CollisionGroup.Weapon;
		SetInteractsAs( CollisionLayer.Debris );
	}

	WireHUD wirePanel;

	public override void CreateHudElements()
	{
		base.CreateHudElements();
		wirePanel = new WireHUD(this);
	}

	public override void DestroyHudElements()
	{
		base.DestroyHudElements();
		wirePanel?.Delete();
		wirePanel = null;
	}

	[Net]
	public Entity selectedEntity {get; set;}
	[Net]
	public string selectedID {get; set;}

	[Net, Predicted]
	public Entity wireTarget {get; set;}
	public override void Simulate( Client client )
	{
		if ( Owner is not Player owner ) return;
		var startPos = Owner.EyePos;
        var dir = Owner.EyeRot * Vector3.Forward;
        var tr = Trace.Ray( startPos, startPos + dir * 300 )
					.Ignore( Owner )
					.Run();

		if(!IsClient){
			if(Int32.TryParse(owner.GetClientOwner().GetUserString("wire_selection", "0"), out int j)){
				wire_selection = j;
			}
		}

		if(tr.Entity is IWireEntity)
			wireTarget = tr.Entity;
		else{
			wire_selection = 0;
			if(IsClient){
				wire_select = "0";
				ConsoleSystem.Run("wire_selection 0");
			}
			wireTarget = null;
		}

		using ( Prediction.Off() )
		{
			if(Input.Pressed(InputButton.Reload)){
				selectedEntity = null;
			}
			if (!Input.Pressed( InputButton.Attack1 ) )
				return;
			
			if(wireTarget is null)return;
			if(selectedEntity is null){
				selectedEntity = wireTarget;
				var imputs = (wireTarget as IWireEntity)?.Values().Where(x=>x.direction==WireVal.Direction.Input).ToList();
				if(imputs.Count <= 0) return;
				selectedID = imputs[wire_selection].id;
			}else{
				Log.Info($"Current Index: {wire_selection} {wireTarget} {IsClient}");
				var opputs = (wireTarget as IWireEntity)?.Values().Where(x=>x.direction==WireVal.Direction.Output).ToList();
				if(opputs.Count <= 0) return;
				var targetID = opputs[wire_selection].id;
				WireConnection.MakeConnection(selectedEntity, selectedID, wireTarget, targetID);
				selectedEntity = null;
			}
		}

	}

	public override bool OnMouseWheel(int delta){
		if(wireTarget is not null){
			int MAX = selectedEntity is null
			? (wireTarget as IWireEntity).Values().Where(x=>x.direction==WireVal.Direction.Input).Count()
			: (wireTarget as IWireEntity).Values().Where(x=>x.direction==WireVal.Direction.Output).Count();
			wire_selection = wire_selection + delta;
			if(wire_selection < 0)wire_selection = MAX-1;
			if(wire_selection >= MAX)wire_selection = 0;
			if(wire_selection < 0)wire_selection = 0;
			wire_select = wire_selection.ToString();
			ConsoleSystem.Run("wire_selection "+wire_select);
			return true;
		}
		return false;
	}
}