﻿namespace Sandbox.Tools
{
	[Library( "tool_light", Title = "Lights", Description = "A dynamic point light", Group = "construction" )]
	public partial class LightTool : BaseTool
	{
		PreviewEntity previewModel;

		private string Model => "models/light/light_tubular.vmdl";

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is LightEntity )
				return false;

			if(!this.CanTool())return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Model ) )
			{
				previewModel.RelativeToNormal = false;
				previewModel.OffsetBounds = true;
				previewModel.PositionOffset = -previewModel.CollisionBounds.Center;
			}
		}

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;
			if(!this.CanTool())return;
			using ( Prediction.Off() )
			{
				bool useRope = Input.Pressed( InputButton.Attack1 );
				if ( !useRope && !Input.Pressed( InputButton.Attack2 ) )
					return;

				var tr = (Owner as SandboxPlayer).EyeTrace();

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				if(useRope && !tr.Entity.IsWorld && !Owner.Client.CanTouch(tr.Entity))
					return;

				CreateHitEffects( tr.EndPosition );

				if ( tr.Entity is LightEntity )
				{
					// TODO: Set properties

					return;
				}

				if(!Owner.Client.CanSpawnProp("light/light_tubular.vmdl")){
					Owner.Client.BannedProp("models/light/light_tubular.vmdl");
					return;
				}
				if(!Owner.Client.CanSpawn(PropType.Generic)){
					Owner.Client.HitLimit(PropType.Generic);
					return;
				}

				var light = new LightEntity
				{
					Enabled = true,
					DynamicShadows = false,
					Range = 128,
					Falloff = 1.0f,
					LinearAttenuation = 0.0f,
					QuadraticAttenuation = 1.0f,
					Brightness = 1,
					Color = Color.Random,
					//LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
				};

				light.UseFogNoShadows();
				light.SetModel( Model );
				light.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
				light.Position = tr.EndPosition + -light.CollisionBounds.Center + tr.Normal * light.CollisionBounds.Size * 0.5f;
				light.SetSpawner(Owner.Client, PropType.Generic);
				(Owner as SandboxPlayer)?.undoQueue.Add(new UndoEnt(light));

				if ( !useRope )
					return;

				var rope = Particles.Create( "particles/rope.vpcf" );
				rope.SetEntity( 0, light, Vector3.Down * 6.5f ); // Should be an attachment point

				var attachEnt = tr.Body.IsValid() ? tr.Body.GetEntity() : tr.Entity;
				var attachLocalPos = tr.Body.Transform.PointToLocal( tr.EndPosition ) * (1.0f / tr.Entity.Scale);

				if ( attachEnt.IsWorld )
				{
					rope.SetPosition( 1, attachLocalPos );
				}
				else
				{
					rope.SetEntityBone( 1, attachEnt, tr.Bone, new Transform( attachLocalPos ) );
				}

				var spring = PhysicsJoint.CreateLength( PhysicsPoint.Local( light.PhysicsBody, Vector3.Down * 6.5f ), PhysicsPoint.World( tr.Body, tr.EndPosition ), 100 );
				spring.SpringLinear = new( 5, 0.7f );
				spring.Collisions = true;
				spring.EnableAngularConstraint = false;
				spring.OnBreak += () =>
				{
					rope?.Destroy( true );
					spring.Remove();
				};
			}
		}
	}
}
