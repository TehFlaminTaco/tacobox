using Sandbox;

public partial class Weapon : BaseWeapon, IUse
{
	public virtual int HoldSlot => 1;
	public virtual float ReloadTime => 3.0f;

	public PickupTrigger PickupTrigger { get; protected set; }

	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted]
	public bool IsReloading { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		PickupTrigger.PhysicsBody.AutoSleep = false;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}

	public override void Reload()
	{
		if ( IsReloading )
			return;

		if (!((Owner as SandboxPlayer)?.HasAmmo(Clip1Type)??false) && !((Owner as SandboxPlayer)?.HasAmmo(Clip2Type)??false))
			return;

		if(Clip1 >= Clip1Size && Clip2 >= Clip2Size)
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		(Owner as AnimEntity)?.SetAnimParameter( "b_reload", true );

		StartReloadEffects();
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
		{
			base.Simulate( owner );
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
		if(PartialReload1)
			Clip1 += (Owner as SandboxPlayer)?.RemoveAmmo(Clip1Type, Clip1Size - Clip1)??0;
		else
			Clip1 = (Owner as SandboxPlayer)?.RemoveAmmo(Clip1Type, Clip1Size)??0;
		if(PartialReload2)
			Clip2 += (Owner as SandboxPlayer)?.RemoveAmmo(Clip2Type, Clip2Size - Clip2)??0;
		else
			Clip2 = (Owner as SandboxPlayer)?.RemoveAmmo(Clip2Type, Clip2Size)??0;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );

		// TODO - player third person model reload
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	public bool OnUse( Entity user )
	{
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;

		user.StartTouch( this );

		return false;
	}

	public virtual bool IsUsable( Entity user )
	{
		var player = user as Player;
		if ( Owner != null ) return false;

		if ( player.Inventory is Inventory inventory )
		{
			return inventory.CanAdd( this );
		}

		return true;
	}

	public void Remove()
	{
		//PhysicsGroup?.Wake();
		Delete();
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin();
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		(var pos, var dir) = (Owner as SandboxPlayer).CameraPosition();
		ShootBullet( pos, dir.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		(var pos, var dir) = (Owner as SandboxPlayer).CameraPosition();

		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( pos, dir.Forward, spread, force / numBullets, damage, bulletSize );
		}
	}
}
