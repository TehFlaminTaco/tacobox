using Sandbox;

[Library( "weapon_thunderbuss", Title = "Thunderbuss", Spawnable = true, Group = "Weapon" )]
partial class Thunderbuss : Weapon
{
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float PrimaryRate => 1;
	public override float SecondaryRate => 1;
	public override float ReloadTime => 0.3f;

	public override AmmoType Clip1Type => AmmoType.Shell;
    public override int Clip1Size => 10;
	public override int HoldSlot => 3;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}

		if(!Input.Down(InputButton.Attack1) && !IsReloading){
			if(Clip1==0)return;
			if(Clip1>1){
				DoubleShootEffects();
			}else{
				ShootEffects();
			}
			Owner.Velocity += Owner.EyeRotation.Forward * -150f * Clip1;
			PlaySound( Clip1>1 ? "rust_pumpshotgun.shootdouble" : "rust_pumpshotgun.shoot" );
			ShootBullets( Clip1*10, 0.1f + (0.05f * Clip1), 60.0f, 8.0f, 3.0f );
			Clip1 = 0;
		}

		if(Input.Down(InputButton.Attack1) && !IsReloading && Clip1 < Clip1Size){
			Reload();
		}
	}

    /*public override bool CanSecondaryAttack()
    {
        if ( !Owner.IsValid() || !Input.Down( InputButton.Attack2 ) ) return false;
        if ( Clip1 <= 0 ) return false;

        var rate = SecondaryRate;
        if ( rate <= 0 ) return true;

        return TimeSinceSecondaryAttack > (1 / rate);
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
		PlaySound( "rust_pumpshotgun.shoot" );

		//
		// Shoot the bullets
		//
		ShootBullets( 10, 0.1f, 10.0f, 9.0f, 3.0f );
		Clip1--;
	}

	public override void AttackSecondary()
	{
		if(Clip1 <= 1){
			AttackPrimary();
			return;
		}
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		(Owner as AnimEntity)?.SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		DoubleShootEffects();
		PlaySound( "rust_pumpshotgun.shootdouble" );

		//
		// Shoot the bullets
		//
		ShootBullets( 20, 0.4f, 60.0f, 8.0f, 3.0f );
		Clip1-=2;
	}*/

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimParameter( "fire", true );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 1.0f, 1.5f, 2.0f );
		}

		CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected virtual void DoubleShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire_double", true );
		CrosshairPanel?.CreateEvent( "fire" );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 3.0f, 3.0f, 3.0f );
		}
	}

	public override void OnReloadFinish()
	{
		Clip1+=(Owner as SandboxPlayer)?.RemoveAmmo(Clip1Type, 1)??0;
		IsReloading = false;
		/*if((Owner as SandboxPlayer)?.HasAmmo(Clip1Type)??false && Clip1<Clip1Size){
			Reload();
		}*/

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		FinishReload();
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimParameter( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 ); // TODO this is shit
		anim.SetAnimParameter( "aimat_weight", 1.0f );
	}
}
