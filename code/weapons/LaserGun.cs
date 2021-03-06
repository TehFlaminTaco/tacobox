using Sandbox;

[Library( "weapon_lasergun", Title = "Laser Gun", Spawnable = true, Group = "Weapon" )]
partial class LaserGun : Weapon
{
	public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 5.0f;

	public override AmmoType Clip1Type => AmmoType.Bullet;
    public override bool Clip1Pocket => true;
	public override int HoldSlot => 4;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_smg/rust_smg.vmdl" );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		(Owner as AnimEntity)?.SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "lasergun.pew" );
		(Owner as SandboxPlayer)?.RemoveAmmo(Clip1Type, 1);
		//
		// Shoot the bullets
		//
		ShootBullet( 0.01f, 15.0f, 5.0f, 3.0f );
	}

	public override void AttackSecondary()
	{
		// Grenade lob
	}

	[Event.Frame]
	protected void UpdateBeam(){
		if(TimeSincePrimaryAttack>(1f/PrimaryRate) && Beam is not null){
			Beam.Destroy(true);
			Beam = null;
		}
	}

	Particles Beam;
	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();
		var tr = (Owner as SandboxPlayer).EyeTrace();
	
		Beam ??= Particles.Create( "particles/physgun_beam.vpcf", tr.EndPosition );
		Beam.SetPosition( 1, tr.EndPosition );
		Beam.SetEntityAttachment(0, EffectEntity, "muzzle", true );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
		}

		//ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 2 ); // TODO this is shit
		anim.SetAnimParameter( "aimat_weight", 1.0f );
	}

}
