using System.Linq;
using System.Threading.Tasks;
using Sandbox;

[Library( "tacobox", Title = "TacoBox" )]
partial class SandboxGame : Game
{
	public SandboxGame()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();
		}else{
			Sandbox.ConsoleSystem.Run("bind z undo");
		}

		Gate.ResetGates();
		AdminCore.OnReload();
		Command.SetupCommands();
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		var player = new SandboxPlayer( cl );
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	[ServerCmd( "spawn" )]
	public static async Task Spawn( string modelname )
	{
		var owner = ConsoleSystem.Caller?.Pawn;
		var ownerClient = ConsoleSystem.Caller;

		if ( ConsoleSystem.Caller == null )
			return;

		var modelRotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

		var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		if ( modelname.Count( x => x == '.' ) == 1 && !modelname.EndsWith( ".vmdl", System.StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", System.StringComparison.OrdinalIgnoreCase ) )
		{
			if(!ownerClient.HasFlag("spawnSwerks")){
				ownerClient.BannedProp("S&Werks Props");
				return;
			}
			modelname = await SpawnPackageModel( modelname, tr.EndPosition, modelRotation, owner );
			if ( modelname == null )
				return;
		}else{
			if(!ownerClient.CanSpawnProp(modelname.Substring(7))){
				ownerClient.BannedProp(modelname);
				return;
			}
		}

		if(!ownerClient.CanSpawn(PropType.Prop)){
			ownerClient.HitLimit(PropType.Prop);
			return;
		}
		var model = Model.Load( modelname );
		if ( model == null || model.IsError )
			return;
		var ent = new Prop{
			Model = model
		};
		(owner as SandboxPlayer)?.undoQueue.Add( new UndoEnt(ent) );
		ent.Position = tr.EndPosition;
		ent.Rotation = modelRotation;
		//ent.SetModel( modelname );
		ent.Position = tr.EndPosition - Vector3.Up * ent.CollisionBounds.Mins.z;
		ent.SetSpawner(ownerClient, PropType.Prop);
		ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		if ( !ent.PhysicsBody.IsValid() )
		{
			ent.SetupPhysicsFromOBB( PhysicsMotionType.Dynamic, ent.CollisionBounds.Mins, ent.CollisionBounds.Maxs );
		}
	}

	static async Task<string> SpawnPackageModel( string packageName, Vector3 pos, Rotation rotation, Entity source )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package == null || package.PackageType != Package.Type.Model || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		if ( !source.IsValid ) return null; // source entity died or disconnected or something

		var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );
		var mins = package.GetMeta( "RenderMins", Vector3.Zero );
		var maxs = package.GetMeta( "RenderMaxs", Vector3.Zero );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		return model;
	}

	[ServerCmd( "spawn_entity" )]
	public static void SpawnEntity( string entName )
	{
		var owner = (Player)ConsoleSystem.Caller.Pawn;

		if ( owner == null )
			return;

		var attribute = Library.GetAttribute( entName );

		if ( attribute == null || !attribute.Spawnable )
			return;

		var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 200 )
			.UseHitboxes()
			.Ignore( owner )
			.Size( 2 )
			.Run();

		PropType toSpawn = PropType.Generic;
		var spawnedType = Library.Get<Entity>( entName );
		if(spawnedType.IsAssignableTo(typeof(BaseCarriable)))
			toSpawn = PropType.Weapon;
		if(spawnedType.IsAssignableTo(typeof(CarEntity))||spawnedType.IsAssignableTo(typeof(Chair))) //TODO: group these
			toSpawn = PropType.Vehicle;
		if(spawnedType.IsAssignableTo(typeof(NpcTest)))
			toSpawn = PropType.NPC;
		
		if(!ConsoleSystem.Caller.CanSpawnEnt(entName)){
			ConsoleSystem.Caller.BannedEnt(entName);
			return;
		}

		if(!ConsoleSystem.Caller.CanSpawn(toSpawn)){
			ConsoleSystem.Caller.HitLimit(toSpawn);
			return;
		}

		var ent = Library.Create<Entity>( entName );
		if ( ent is BaseCarriable && owner.Inventory != null )
		{
			if ( owner.Inventory.Add( ent, true ) )
				return;
		}

		ent.Position = tr.EndPosition;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) );
		ent.SetSpawner(ConsoleSystem.Caller, toSpawn);
		(owner as SandboxPlayer)?.undoQueue.Add( new UndoEnt(ent) );
		//Log.Info( $"ent: {ent}" );
	}

	[ServerCmd( "undo" )]
	public static void DoUndo(){
		var owner = ConsoleSystem.Caller.Pawn as SandboxPlayer;
		if (owner is null) return;
		if(owner.undoQueue.DoUndo()){
			Sound.FromEntity("balloon_pop_cute", owner);
		}
	}
	public override void DoPlayerNoclip(Client player){
		DoPlayerNoclip(player, false);
	}

	public void DoPlayerNoclip( Client player, bool force = false  )
	{
		if(!player.HasCommand("noclip")) return;
		if ( player.Pawn is Player basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				basePlayer.DevController = null;
			}
			else
			{
				basePlayer.DevController = new NoclipController();
			}
		}
	}
}
