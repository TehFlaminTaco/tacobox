using Sandbox;
using System;
using System.Collections.Generic;

[Library( "ent_chair", Title = "Chair", Spawnable = true )]
public partial class Chair : Prop, IUse, IWireEntity
{
    private TimeSince timeSinceDriverLeft;
    [Net] public Player driver { get; private set; }

	public List<WireVal> outputs;
	[Net]
	public double W {get; set;}
	[Net]
	public double A {get; set;}
	[Net]
	public double S {get; set;}
	[Net]
	public double D {get; set;}

    private void RemoveDriver( SandboxPlayer player )
	{
		driver = null;
		player.Vehicle = null;
		player.VehicleController = null;
		player.VehicleAnimator = null;
		player.VehicleCamera = null;
		player.Parent = null;
		player.PhysicsBody.Enabled = true;
		player.PhysicsBody.Position = player.Position;

		W = 0;
		A = 0;
		S = 0;
		D = 0;

		timeSinceDriverLeft = 0;
	}

    public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/chair01.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		if ( user is SandboxPlayer player && player.Vehicle == null && timeSinceDriverLeft > 1.0f )
		{
			player.Vehicle = this;
			player.VehicleController = new ChairController();
			player.VehicleAnimator = new ChairAnimator();
			player.VehicleCamera = new ChairCamera();
			player.Parent = this;
			player.LocalPosition = Vector3.Up * 6 + Vector3.Forward * 3;
			player.LocalRotation = Rotation.Identity;
			player.LocalScale = 1;
			player.PhysicsBody.Enabled = false;

			driver = player;
		}

		return true;
	}

    protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( driver is SandboxPlayer player )
		{
			RemoveDriver( player );
		}
	}

    [Event.Tick.Server]
	protected void Tick()
	{
		if ( driver is SandboxPlayer player )
		{
			if ( player.LifeState != LifeState.Alive || player.Vehicle != this )
			{
				RemoveDriver( player );
			}
		}
	}

    public override void Simulate( Client owner )
	{
		if ( owner == null ) return;
		if ( !IsServer ) return;

		using ( Prediction.Off() )
		{

			if ( Input.Pressed( InputButton.Use ) )
			{
				if ( owner.Pawn is SandboxPlayer player && !player.IsUseDisabled() )
				{
					RemoveDriver( player );

					return;
				}
			}

			W = Input.Down(InputButton.Forward) ? 1 : 0;
			A = Input.Down(InputButton.Left) ? 1 : 0;
			S = Input.Down(InputButton.Back) ? 1 : 0;
			D = Input.Down(InputButton.Right) ? 1 : 0;
		}
	}

	public List<WireVal> Values() {
		if(outputs is not null) return outputs;
		outputs = new();
		outputs.Add(new WireValNormal("w", "W", WireVal.Direction.Output, ()=>W, f=>W=f));
		outputs.Add(new WireValNormal("a", "A", WireVal.Direction.Output, ()=>A, f=>A=f));
		outputs.Add(new WireValNormal("s", "S", WireVal.Direction.Output, ()=>S, f=>S=f));
		outputs.Add(new WireValNormal("d", "D", WireVal.Direction.Output, ()=>D, f=>D=f));
		return outputs;
	}
}