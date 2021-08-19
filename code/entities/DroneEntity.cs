using Sandbox;
using System;
using System.Collections.Generic;

[Library( "ent_drone", Title = "Drone", Spawnable = true )]
public partial class DroneEntity : Prop, IWireEntity
{
	public virtual float altitudeAcceleration => 2000;
	public virtual float movementAcceleration => 5000;
	public virtual float yawSpeed => 15550;
	public virtual float uprightSpeed => 5000;
	public virtual float uprightDot => 0.5f;
	public virtual float leanWeight => 0.5f;
	public virtual float leanMaxVelocity => 1000;

	/*private struct DroneInputState
	{
		public Vector3 movement;
		public float throttle;
		public float pitch;
		public float yaw;

		public void Reset()
		{
			movement = Vector3.Zero;
			pitch = 0;
			yaw = 0;
		}
	}

	private DroneInputState currentInput;*/

	[Net]
	public Vector3 movement {get; set;} = Vector3.Zero;
	[Net]
	public float throttle {get; set;} = 0f;
	[Net]
	public float yaw {get; set;} = 0f;
	[Net]
	public float pitch {get; set;} = 0f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "entities/drone/drone.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	[Event.Physics.PostStep]
	public void OnPostPhysicsStep()
	{
		if ( !PhysicsBody.IsValid() )
		{
			return;
		}

		if(movement.LengthSquared > 1)
			movement = movement.Normal;
		throttle = throttle.Clamp(-1f, 1f);

		var body = PhysicsBody;
		var transform = Transform;

		body.LinearDrag = 1.0f;
		body.AngularDrag = 1.0f;
		body.LinearDamping = 4.0f;
		body.AngularDamping = 4.0f;

		var yawRot = Rotation.From( new Angles( 0, Rotation.Angles().yaw, 0 ) );
		var worldMovement = yawRot * movement;
		var velocityDirection = body.Velocity.WithZ( 0 );
		var velocityMagnitude = velocityDirection.Length;
		velocityDirection = velocityDirection.Normal;

		var velocityScale = (velocityMagnitude / leanMaxVelocity).Clamp( 0, 1 );
		var leanDirection = worldMovement.LengthSquared == 0.0f
			? -velocityScale * velocityDirection
			: worldMovement;

		var targetUp = (Vector3.Up + leanDirection * leanWeight * velocityScale).Normal;
		var currentUp = transform.NormalToWorld( Vector3.Up );
		var alignment = Math.Max( Vector3.Dot( targetUp, currentUp ), 0 );

		bool hasCollision = false;
		bool isGrounded = false;

		if ( !hasCollision || isGrounded )
		{
			var hoverForce = isGrounded && throttle <= 0 ? Vector3.Zero : -1 * transform.NormalToWorld( Vector3.Up ) * -800.0f;
			var movementForce = isGrounded ? Vector3.Zero : worldMovement * movementAcceleration;
			var altitudeForce = transform.NormalToWorld( Vector3.Up ) * throttle * altitudeAcceleration;
			var totalForce = hoverForce + movementForce + altitudeForce;
			body.ApplyForce( (totalForce * alignment) * body.Mass );
		}

		if ( !hasCollision && !isGrounded )
		{
			var spinTorque = Transform.NormalToWorld( new Vector3( 0, 0, yaw * yawSpeed ) );
			var uprightTorque = Vector3.Cross( currentUp, targetUp ) * uprightSpeed;
			var uprightAlignment = alignment < uprightDot ? 0 : alignment;
			var totalTorque = spinTorque * alignment + uprightTorque * uprightAlignment;
			body.ApplyTorque( (totalTorque * alignment) * body.Mass );
		}
	}

	public override void Simulate( Client owner )
	{
		if ( owner == null ) return;
		if ( !IsServer ) return;
	}

	private readonly Vector3[] turbinePositions = new Vector3[]
	{
		new Vector3( -35.37f, 35.37f, 10.0f ),
		new Vector3( 35.37f, 35.37f, 10.0f ),
		new Vector3( 35.37f, -35.37f, 10.0f ),
		new Vector3( -35.37f, -35.37f, 10.0f )
	};

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( IsClient )
		{
		}
	}

	private float spinAngle;

	[Event.Frame]
	public void OnFrame()
	{
		spinAngle += 10000.0f * Time.Delta;
		spinAngle %= 360.0f;

		for ( int i = 0; i < turbinePositions.Length; ++i )
		{
			var transform = Transform.ToWorld( new Transform( turbinePositions[i] * Scale, Rotation.From( new Angles( 0, spinAngle, 0 ) ) ) );
			transform.Scale = Scale;
			SetBoneTransform( i, transform );
		}
	}

	List<WireVal> values;
	public List<WireVal> Values()
	{
		if(values is not null)return values;

		values = new();
		values.Add(new WireValVector("Thrust Direction", "Thrust Direction", WireVal.Direction.Input, ()=>movement, f=>movement = f));
		values.Add(new WireValNormal("Throttle", "Throttle", WireVal.Direction.Input, ()=>throttle, f=>throttle=(float)f));
		values.Add(new WireValNormal("Yaw", "Yaw", WireVal.Direction.Input, ()=>yaw, f=>yaw=(float)f));
		values.Add(new WireValNormal("Pitch", "Pitch", WireVal.Direction.Input, ()=>pitch, f=>pitch=(float)f));
		values.Add(new WireValVector("Position", "Position", WireVal.Direction.Output, ()=>Position, f=>{}));
		values.Add(new WireValRotation("Rotation", "Rotation", WireVal.Direction.Output, ()=>Rotation, f=>{}));
		return values;
	}
}
