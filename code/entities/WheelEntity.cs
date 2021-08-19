using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.Joints;

[Library( "ent_wheel" )]
public partial class WheelEntity : Prop, IWireEntity
{
	public RevoluteJoint Joint;
	[Net]
	public float torque {get; set;} = 1000.0f;
	[Net]
	public float max_speed {get; set;} = 10.0f;

	[Net]
	public float wireTorque {get; set;} = 0.0f;


	public List<WireVal> values;
	public List<WireVal> Values()
	{
		if(values is not null) return values;
		values = new();
		values.Add(new WireValNormal("torque", "Torque", WireVal.Direction.Input, ()=>wireTorque, f=>wireTorque=(float)f));
		return values;
	}

	[Event.Physics.PostStep]
	public virtual void OnPostPhysicsStep()
	{
		if(PhysicsBody.IsValid()){
			var currentSpeed = PhysicsBody.AngularVelocity.Dot(this.Rotation.Right);
			var speedDif = (max_speed * wireTorque) - currentSpeed;
			var maxedSpeed = speedDif < 0 ? Math.Max(speedDif * 30000, -torque*100) : Math.Min(speedDif * 30000, torque*100);
			PhysicsBody.ApplyAngularImpulse(this.Rotation.Right * maxedSpeed);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Joint.IsValid )
		{
			Joint.Remove();
		}
	}

	protected override void UpdatePropData( Model model )
	{
		base.UpdatePropData( model );

		Health = -1;
	}

	bool IWireEntity.ShouldGlow => true;
}
