

using System.Linq;
using System;
using Sandbox;

[Library( "wiregun", Title = "Wire Gun",  Spawnable = true, Group = "Wire" )]
public partial class WireGun : Carriable {
	[ConVar.ClientData("wire_selection")]
	public static string wire_select {get; set;}
	public static int wire_selection {get; set;} = 0;
	public override int HoldSlot => 0;

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

	public Entity selectedEntity {get; set;}
	public string selectedID {get; set;}
	public override void Simulate( Client client )
	{
		if ( Owner is not Player owner ) return;
		if(IsServer)return;

		if(Input.Pressed(InputButton.Reload)){
			selectedEntity = null;
		}
		if(selectedEntity is not null && !selectedEntity.IsValid()){
			selectedEntity = null;
		}

		if (Input.Pressed( InputButton.Attack1 ) ){
			if(WireHUD.HoveredEntity is not null){
				if(selectedEntity is null){
					selectedEntity = WireHUD.HoveredEntity;
					selectedID = WireHUD.HoveredKey;
				}else{
					var builtString = $"{selectedEntity.NetworkIdent}:{selectedID}:{WireHUD.HoveredEntity.NetworkIdent}:{WireHUD.HoveredKey}";
					WireConnection.MakeConnection(builtString);
					selectedEntity = null;
				}
			}
		}

	}
}