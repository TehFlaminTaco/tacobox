using Sandbox;

public partial class AmmoBox : Prop {
    public virtual AmmoType PickupType => AmmoType.None;
    public virtual int PickupAmount => 0;

    public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/cardboardbox01.vmdl" );
        Scale = 0.5f;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
    }

    protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if(eventData.Entity is SandboxPlayer sp){
            sp.AddAmmo(PickupType, PickupAmount);
            Delete();
        }
	}

}

[Library( "ent_pistolammo", Title = "Pistol Ammo", Spawnable = true )]
public partial class PistolAmmo : AmmoBox
{
	public override AmmoType PickupType => AmmoType.Bullet;

	public override int PickupAmount => 100;
}

[Library( "ent_shotgunammo", Title = "Shotgun Ammo", Spawnable = true )]
public partial class ShotgunAmmo : AmmoBox
{
	public override AmmoType PickupType => AmmoType.Shell;

	public override int PickupAmount => 100;
}