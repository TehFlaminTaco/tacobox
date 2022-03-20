using Sandbox;
using System;

public class ChairCamera : CameraMode
{
	protected virtual float MinFov => 80.0f;
	protected virtual float MaxFov => 100.0f;
	protected virtual float MaxFovSpeed => 1000.0f;
	protected virtual float FovSmoothingSpeed => 4.0f;
	protected virtual float OrbitCooldown => 0.6f;
	protected virtual float OrbitSmoothingSpeed => 25.0f;
	protected virtual float OrbitReturnSmoothingSpeed => 4.0f;
	protected virtual float MinOrbitPitch => -25.0f;
	protected virtual float MaxOrbitPitch => 70.0f;
	protected virtual float FixedOrbitPitch => 10.0f;
	protected virtual float OrbitHeight => 35.0f;
	protected virtual float OrbitDistance => 150.0f;
	protected virtual float MaxOrbitReturnSpeed => 100.0f;
	protected virtual float MinChairPitch => -60.0f;
	protected virtual float MaxChairPitch => 60.0f;
	protected virtual float FirstPersonPitch => 10.0f;
	protected virtual float ChairPitchSmoothingSpeed => 1.0f;
	protected virtual float CollisionRadius => 8.0f;
	protected virtual float ShakeSpeed => 10.0f;
	protected virtual float ShakeSpeedThreshold => 1500.0f;
	protected virtual float ShakeMaxSpeed => 2500.0f;
	protected virtual float ShakeMaxLength => 1.0f;

	private bool orbitEnabled;
	private TimeSince timeSinceOrbit;
	private Angles orbitAngles;
	private Rotation orbitYawRot;
	private Rotation orbitPitchRot;
	private float currentFov;
	private float chairPitch;
	private bool firstPerson;

	public override void Activated()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		orbitEnabled = false;
		timeSinceOrbit = 0.0f;
		orbitAngles = Angles.Zero;
		orbitYawRot = Rotation.Identity;
		orbitPitchRot = Rotation.Identity;
		currentFov = MinFov;
		chairPitch = 0;
		firstPerson = false;

		var chair = (pawn as SandboxPlayer)?.Vehicle as Chair;
		if ( !chair.IsValid() ) return;

		orbitYawRot = firstPerson ? Rotation.Identity : Rotation.FromYaw( chair.Rotation.Yaw() );
		orbitPitchRot = firstPerson ? Rotation.FromPitch( FirstPersonPitch ) : Rotation.Identity;
		orbitAngles = (orbitYawRot * orbitPitchRot).Angles();
	}

	public override void Update()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		var chair = (pawn as SandboxPlayer)?.Vehicle as Chair;
		if ( !chair.IsValid() ) return;

		var body = chair.PhysicsBody;
		if ( !body.IsValid() )
			return;

		if ( orbitEnabled && timeSinceOrbit > OrbitCooldown )
			orbitEnabled = false;

		var chairRot = chair.Rotation;
		chairPitch = chairPitch.LerpTo(chairRot.Pitch().Clamp( MinChairPitch, MaxChairPitch ), Time.Delta * ChairPitchSmoothingSpeed );

		if ( orbitEnabled )
		{
			var slerpAmount = Time.Delta * OrbitSmoothingSpeed;

			orbitYawRot = Rotation.Slerp( orbitYawRot, Rotation.From( 0.0f, orbitAngles.yaw, 0.0f ), slerpAmount );
			orbitPitchRot = Rotation.Slerp( orbitPitchRot, Rotation.From( orbitAngles.pitch + chairPitch, 0.0f, 0.0f ), slerpAmount );
		}
		else
		{
			if ( firstPerson )
			{
				var targetYaw = 0;
				var targetPitch = FirstPersonPitch;
				var slerpAmount = Time.Delta * OrbitReturnSmoothingSpeed;

				orbitYawRot = Rotation.Slerp( orbitYawRot, Rotation.FromYaw( targetYaw ), slerpAmount );
				orbitPitchRot = Rotation.Slerp( orbitPitchRot, Rotation.FromPitch( targetPitch ), slerpAmount );
			}
			else
			{
				var targetPitch = FixedOrbitPitch.Clamp( MinOrbitPitch, MaxOrbitPitch );
				var targetYaw = !firstPerson ? chairRot.Yaw() : chairRot.Yaw();
				var slerpAmount = MaxOrbitReturnSpeed > 0.0f ? Time.Delta.Clamp( 0.0f, OrbitReturnSmoothingSpeed ) : 1.0f;

				orbitYawRot = Rotation.Slerp( orbitYawRot, Rotation.FromYaw( targetYaw ), slerpAmount );
				orbitPitchRot = Rotation.Slerp( orbitPitchRot, Rotation.FromPitch( targetPitch + chairPitch ), slerpAmount );
			}

			orbitAngles.pitch = orbitPitchRot.Pitch();
			orbitAngles.yaw = orbitYawRot.Yaw();
			orbitAngles = orbitAngles.Normal;
		}

		if ( firstPerson )
		{
			DoFirstPerson();
		}
		else
		{
			DoThirdPerson( chair, body );
		}

		currentFov = MaxFovSpeed > 0.0f ? currentFov.LerpTo( MinFov, Time.Delta * FovSmoothingSpeed ) : MaxFov;
		FieldOfView = currentFov;
	}

	private void DoFirstPerson()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		Position = pawn.EyePosition;
		Rotation = pawn.Rotation * (orbitYawRot * orbitPitchRot);

		Viewer = pawn;
	}

	private void DoThirdPerson( Chair chair, PhysicsBody body )
	{
		Rotation = orbitYawRot * orbitPitchRot;

		var chairPos = chair.Position + chair.Rotation * (body.LocalMassCenter * chair.Scale);
		var startPos = chairPos;
		var targetPos = startPos + Rotation.Backward * (OrbitDistance * chair.Scale) + (Vector3.Up * (OrbitHeight * chair.Scale));

		var tr = Trace.Ray( startPos, targetPos )
			.Ignore( chair )
			.Radius( Math.Clamp( CollisionRadius * chair.Scale, 2.0f, 10.0f ) )
			.WorldOnly()
			.Run();

		Position = tr.EndPosition;

		Viewer = null;
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		var chair = (pawn as SandboxPlayer)?.Vehicle as Chair;
		if ( !chair.IsValid() ) return;

		if ( input.Pressed( InputButton.View ) )
		{
			firstPerson = !firstPerson;
			orbitYawRot = firstPerson ? Rotation.Identity : Rotation.FromYaw( chair.Rotation.Yaw() );
			orbitPitchRot = firstPerson ? Rotation.FromPitch( FirstPersonPitch ) : Rotation.Identity;
			orbitAngles = (orbitYawRot * orbitPitchRot).Angles();
			orbitEnabled = false;
			timeSinceOrbit = 0.0f;
		}

		if ( (Math.Abs( input.AnalogLook.pitch ) + Math.Abs( input.AnalogLook.yaw )) > 0.0f )
		{
			if ( !orbitEnabled )
			{
				orbitAngles = (orbitYawRot * orbitPitchRot).Angles();
				orbitAngles = orbitAngles.Normal;

				orbitYawRot = Rotation.From( 0.0f, orbitAngles.yaw, 0.0f );
				orbitPitchRot = Rotation.From( orbitAngles.pitch, 0.0f, 0.0f );
			}

			orbitEnabled = true;
			timeSinceOrbit = 0.0f;

			orbitAngles.yaw += input.AnalogLook.yaw;
			orbitAngles.pitch += input.AnalogLook.pitch;
			orbitAngles = orbitAngles.Normal;
			orbitAngles.pitch = orbitAngles.pitch.Clamp( MinOrbitPitch, MaxOrbitPitch );
		}

		if ( firstPerson )
		{
			input.ViewAngles = (chair.Rotation * Rotation.From( orbitAngles )).Angles();
		}
		else
		{
			input.ViewAngles = orbitEnabled ? orbitAngles : chair.Rotation.Angles();
		}

		input.ViewAngles = input.ViewAngles.Normal;
	}

	private void ApplyShake( float speed )
	{
		if ( speed < ShakeSpeedThreshold )
			return;

		var pos = (Time.Now % MathF.PI) * ShakeSpeed;
		var length = (speed - ShakeSpeedThreshold) / (ShakeMaxSpeed - ShakeSpeedThreshold);
		length = length.Clamp( 0, ShakeMaxLength );

		float x = Noise.Perlin( pos, 0, 0 ) * length;
		float y = Noise.Perlin( pos, 5.0f, 0 ) * length;

		Position += Rotation.Right * x + Rotation.Up * y;
		Rotation *= Rotation.FromAxis( Vector3.Up, x );
		Rotation *= Rotation.FromAxis( Vector3.Right, y );
	}
}

