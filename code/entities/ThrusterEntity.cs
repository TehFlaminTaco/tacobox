using System.Collections.Generic;
using Sandbox;

[Library( "ent_thruster" )]
partial class ThrusterEntity : Prop, IWireEntity
{
	[Net]
	public float Force {get; set;} = 1000.0f;

	public bool Massless = false;
	public PhysicsBody TargetBody;

	[Net]
	public bool Enabled { get; set; } = true;

	[Event.Physics.PostStep]
	public virtual void OnPostPhysicsStep()
	{
		double force = wireForce;
		Enabled = force != 0;
		if ( IsServer && Enabled )
		{
			if ( TargetBody.IsValid() )
			{
				TargetBody.ApplyForceAt( Position, Rotation.Down * (Massless ? Force * TargetBody.Mass : Force) * (float)force );
			}
			else if ( PhysicsBody.IsValid() )
			{
				PhysicsBody.ApplyForce( Rotation.Down * (Massless ? Force * PhysicsBody.Mass : Force) * (float)force );
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsClient )
		{
			KillEffects();
		}
	}

	[Net]
	public float wireForce { get; set; }
	List<WireVal> values;
	public List<WireVal> Values() {
		if(values is not null) return values;
		values = new();
		values.Add(new WireValNormal("force", "Force", WireVal.Direction.Input, ()=>wireForce, f=>wireForce=(float)f));
		return values;
	}

	bool IWireEntity.ShouldGlow => true;
}
