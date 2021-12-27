using Sandbox;
using System.Collections.Generic;

partial class SandboxPlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	private DamageInfo lastDamage;

	[Net] public PawnController VehicleController { get; set; }
	[Net] public PawnAnimator VehicleAnimator { get; set; }
	[Net, Predicted] public ICamera VehicleCamera { get; set; }
	[Net, Predicted] public Entity Vehicle { get; set; }
	[Net, Predicted] public ICamera MainCamera { get; set; }

	public Clothing.Container Clothing = new();

	[Net] public bool GodMode {get; set;}
	public SmartSnap smartSnap = new();

	public UndoQueue undoQueue;

	public ICamera LastCamera { get; set; }

	public static SoundEvent FallDamage = new SoundEvent("sounds/physics/bullet_impacts/flesh_npc_04.vsnd");
	public SandboxPlayer()
	{
		Inventory = new Inventory( this );
		undoQueue = new UndoQueue( this );
	}

	public SandboxPlayer( Client cl ) : this()
	{
		Clothing.LoadFromClient( cl );
	}


	public override void Spawn()
	{
		MainCamera = new FirstPersonCamera();
		LastCamera = MainCamera;

		base.Spawn();
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new TacoWalkController();
		Animator = new StandardPlayerAnimator();

		MainCamera = LastCamera;
		Camera = MainCamera;

		if ( DevController is NoclipController )
		{
			DevController = null;
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		if(IsClient)
			InventoryBar.Instance.ReBuild();

		Inventory.Add( new PhysGun(), true );
		Inventory.Add( new GravGun() );
		Inventory.Add( new Tool() );
		Inventory.Add( new Pistol() );
		Inventory.Add( new Flashlight() );

		FallDamage.Sounds = new List<string>(new[]{
			"sounds/physics/bullet_impacts/flesh_npc_01.vsnd",
			"sounds/physics/bullet_impacts/flesh_npc_02.vsnd",
			"sounds/physics/bullet_impacts/flesh_npc_03.vsnd",
			"sounds/physics/bullet_impacts/flesh_npc_04.vsnd",
			"sounds/physics/bullet_impacts/flesh_npc_05.vsnd",
			"sounds/physics/bullet_impacts/flesh_npc_06.vsnd",
			"sounds/physics/bullet_impacts/flesh_npc_07.vsnd",
			"sounds/physics/bullet_impacts/flesh_npc_08.vsnd",
		});

		base.Respawn();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		VehicleController = null;
		VehicleAnimator = null;
		VehicleCamera = null;
		Vehicle = null;

		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );
		LastCamera = MainCamera;
		MainCamera = new SpectateRagdollCamera();
		Camera = MainCamera;
		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		Velocity += info.Force;
		if(GodMode)return;

		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;
		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );
		base.TakeDamage( info );
	}

	[ClientRpc]
	public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
	}

	public override PawnController GetActiveController()
	{
		if ( VehicleController != null ) return VehicleController;
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override PawnAnimator GetActiveAnimator()
	{
		if ( VehicleAnimator != null ) return VehicleAnimator;

		return base.GetActiveAnimator();
	}

	public ICamera GetActiveCamera()
	{
		if ( VehicleCamera != null ) return VehicleCamera;

		return MainCamera;
	}

	Rotation lastFreeRotation = Rotation.Identity;
	public override void FrameSimulate(Client cl){
		if(ActiveChild is Tool tool && (tool.CurrentTool?.EyeLock()??false)){
			EyeRot = lastFreeRotation;
			Input.Rotation = EyeRot;
			Input.MouseDelta = Vector3.Zero;
		}else{
			lastFreeRotation = EyeRot;
		}
		base.FrameSimulate(cl);
	}

	public override void Simulate( Client cl )
	{
		if(ActiveChild is Tool tool && (tool.CurrentTool?.EyeLock()??false)){
			EyeRot = lastFreeRotation;
			Input.Rotation = EyeRot;
			Input.MouseDelta = Vector3.Zero;
		}else{
			lastFreeRotation = EyeRot;
		}
		lastEyeTrace = null;
		base.Simulate( cl );
		if(IsServer && MainCamera is ThirdPersonCameraTracked tr)tr.Update();

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		if ( VehicleController != null && DevController is NoclipController )
		{
			DevController = null;
		}

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( MainCamera is not FirstPersonCamera )
			{
				MainCamera = new FirstPersonCamera();
			}
			else
			{
				MainCamera = new ThirdPersonCameraTracked{Pawn = this};
			}
		}

		Camera = GetActiveCamera();

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 500.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if ( Input.Released( InputButton.Jump ) )
		{
			if ( timeSinceJumpReleased < 0.3f )
			{
				Game.Current?.DoPlayerNoclip( cl );
			}

			timeSinceJumpReleased = 0;
		}

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	[ServerCmd( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( !slot.ClassInfo.IsNamed( entName ) )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}

	public (Vector3 position, Rotation angle) CameraPosition(){
		if(Camera is ThirdPersonCameraTracked tr){
			return (tr.Pos, tr.Rot);
		}else{
			return (EyePos, EyeRot);
		}
	}

	TraceResult? lastEyeTrace = null;
	public TraceResult EyeTrace(){
		if(lastEyeTrace is not null){
			return (TraceResult)lastEyeTrace;
		}
		(var pos, var ang) = CameraPosition();
		var trace = Trace.Ray( pos, pos + ang.Forward * 10000.0f )
			.UseHitboxes()
			.Ignore( this, false )
			.HitLayer( CollisionLayer.Debris )
			.Run();

		if(ActiveChild is Tool){
			var snapTrace = SmartSnap.SnapSurface(trace);
			if(Input.Down(InputButton.Use))
				trace = snapTrace;
		}

		lastEyeTrace = trace;
		return trace;
	}

	[Event.Frame]
	public void ResetTraceOnFrame(){
		lastEyeTrace = null;
	}
}
