using Sandbox;

[Library( "weapon_lasergun", Title = "Laser Gun", Spawnable = true, Group = "Weapon" )]
partial class LaserGun : Weapon
{
	public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 5.0f;

	public override AmmoType Clip1Type => AmmoType.Bullet;
    public override int Clip1Size => 50;
	public override int HoldSlot => 4;

	public override void Spawn()
	{
		base.Spawn();

		Clip1 = 50;

		SetModel( "weapons/rust_smg/rust_smg.vmdl" );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "lasergun.pew" );
		Clip1--;
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
		var startPos = Owner.EyePos;
		var dir = Owner.EyeRot.Forward;
		var tr = Trace.Ray( startPos, startPos + dir * 5000f )
			.UseHitboxes()
			.Ignore( Owner )
			.Run();
		Beam ??= Particles.Create( "particles/physgun_beam.vpcf", tr.EndPos );
		Beam.SetPosition( 1, tr.EndPos );
		Beam.SetEntityAttachment(0, EffectEntity, "muzzle", true );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		if ( Owner == Local.Pawn )
		{
			//new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
		}

		//ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}

}
