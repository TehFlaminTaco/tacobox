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
		var player = new SandboxPlayer();
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	[ServerCmd( "spawn" )]
	public static void Spawn( string modelname )
	{
		var owner = ConsoleSystem.Caller?.Pawn;

		if ( ConsoleSystem.Caller == null )
			return;

		var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		var ent = new Prop();
		ent.Position = tr.EndPos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );
		ent.SetModel( modelname );
		ent.Position = tr.EndPos - Vector3.Up * ent.CollisionBounds.Mins.z;
		(owner as SandboxPlayer)?.undoQueue.Add( new UndoEnt(ent) );
	}

	[ServerCmd( "spawn_entity" )]
	public static void SpawnEntity( string entName )
	{
		var owner = ConsoleSystem.Caller.Pawn;

		if ( owner == null )
			return;

		var attribute = Library.GetAttribute( entName );

		if ( attribute == null || !attribute.Spawnable )
			return;

		var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 200 )
			.UseHitboxes()
			.Ignore( owner )
			.Size( 2 )
			.Run();

		var ent = Library.Create<Entity>( entName );
		if ( ent is BaseCarriable && owner.Inventory != null )
		{
			if ( owner.Inventory.Add( ent, true ) )
				return;
		}

		ent.Position = tr.EndPos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) );
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
				Log.Info( "Noclip Mode Off" );
				basePlayer.DevController = null;
			}
			else
			{
				Log.Info( "Noclip Mode On" );
				basePlayer.DevController = new NoclipController();
			}
		}
	}
}
