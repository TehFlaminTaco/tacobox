using System.Collections.Generic;
using Sandbox;

[Library( "weapon_rocketlauncher", Title = "Rocket Launcher", Spawnable = true, Group = "Weapon" )]
partial class RocketLauncher : Weapon, IWireEntity
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	public TimeSince TimeSinceDischarge { get; set; }

	public override AmmoType Clip1Type => AmmoType.Rocket;
    public override int Clip1Size => 5;
	public override int HoldSlot => 5;

	public virtual float Damage => 9.0f;
	public virtual float Force => 1.5f;

	public override void Spawn()
	{
		base.Spawn();

		Clip1 = 5;

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
		
		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		//ShootBullet( 0.05f, Force, Damage, 3.0f );
		if(IsServer){
			var startPos = Owner.EyePos + Owner.EyeRot.Forward * 30f;
			var dir = Rotation.LookAt(((Owner as SandboxPlayer).EyeTrace().EndPos - startPos).Normal);
			var rocket = new Rocket();
			rocket.Position = startPos;
			rocket.Rotation = dir.RotateAroundAxis(Vector3.Right, 90f);
			rocket.ApplyAbsoluteImpulse(dir.Forward * 30000f);
		}
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
		//ShootBullet( pos, rot.Forward, 0.05f, Force, Damage, 3.0f );

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

partial class Rocket : Prop {
	float armTime = 0.0f;
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_props/gas_cylinder_tall/gas_cylinder_tall.vmdl" );
		Scale = 0.2f;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		armTime = Time.Now + 0.1f;
    }

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if(Time.Now > armTime)
			TakeDamage(DamageInfo.Generic(90000f));
	}
}