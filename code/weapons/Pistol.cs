using System.Collections.Generic;
using Sandbox;

[Library( "weapon_pistol", Title = "Pistol", Spawnable = true, Group = "Weapon" )]
partial class Pistol : Weapon, IWireEntity
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	public TimeSince TimeSinceDischarge { get; set; }

	public override AmmoType Clip1Type => AmmoType.Bullet;
    public override int Clip1Size => 10;
	public override int HoldSlot => 2;

	public virtual float Damage => 9.0f;
	public virtual float Force => 1.5f;

	public override void Spawn()
	{
		base.Spawn();

		Clip1 = 10;

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 ) && Clip1 > 0;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		
		(Owner as AnimEntity)?.SetAnimParameter( "b_attack", true );

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		ShootBullet( 0.05f, Force, Damage, 3.0f );
		Clip1--;
	}

	private void Discharge()
	{
		if ( TimeSinceDischarge < 0.5f )
			return;

		TimeSinceDischarge = 0;

		var muzzle = GetAttachment( "muzzle" ) ?? default;
		var pos = muzzle.Position;
		var rot = muzzle.Rotation;

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		ShootBullet( pos, rot.Forward, 0.05f, Force, Damage, 3.0f );

		ApplyAbsoluteImpulse( rot.Backward * 200.0f );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Speed > 500.0f )
		{
			Discharge();
		}
	}

	[Event.Tick]
	protected void WireFire(){
		if(fire > 0.0f && !IsClient)
			Discharge();
	}

	[Net]
	public float fire {get; set;}
	public List<WireVal> values;
	public List<WireVal> Values()
	{
		if(values is not null) return values;
		values = new();
		values.Add(new WireValNormal("fire", "Fire", WireVal.Direction.Input, ()=>fire, f=>fire=(float)f));
		return values;
	}
}
