namespace Sandbox.Tools
{
	[Library( "tool_balloon", Title = "Balloons", Description = "Create Balloons!", Group = "construction" )]
	public partial class BalloonTool : BaseTool
	{
		[Net]
		public Color32 Tint { get; set; }

		PreviewEntity previewModel;

		public override void Activate()
		{
			base.Activate();

			if ( Host.IsServer )
			{
				Tint = Color.Random.ToColor32();
			}
		}

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if (!this.CanTool())
				return false;

			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is BalloonEntity )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, "models/citizen_props/balloonregular01.vmdl" ) )
			{
				previewModel.RelativeToNormal = false;
			}
		}

		public override void Simulate()
		{
			if ( previewModel.IsValid() )
			{
				previewModel.RenderColor = Tint.ToColor();
			}

			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				bool useRope = Input.Pressed( InputButton.Attack1 );
				if ( !useRope && !Input.Pressed( InputButton.Attack2 ) )
					return;

				if (!this.CanTool())
					return;

				var tr = (Owner as SandboxPlayer).EyeTrace();

				if ( !tr.Hit )
					return;

				if ( !tr.Entity.IsValid() )
					return;

				if(useRope && !tr.Entity.IsWorld && !Owner.Client.CanTouch(tr.Entity))
					return;

				CreateHitEffects( tr.EndPosition );

				if ( tr.Entity is BalloonEntity )
					return;
				
				if(!Owner.Client.CanSpawnProp("citizen_props/balloonregular01.vmdl")){
					Owner.Client.BannedProp("models/citizen_props/balloonregular01.vmdl");
					return;
				}

				if(!Owner.Client.CanSpawn(PropType.Generic)){
					Owner.Client.HitLimit(PropType.Generic);
					return;
				}

				var ent = new BalloonEntity
				{
					Position = tr.EndPosition,
				};

				ent.SetModel( "models/citizen_props/balloonregular01.vmdl" );
				ent.PhysicsBody.GravityScale = -0.2f;
				ent.RenderColor = Tint.ToColor();
				ent.SetSpawner(Owner.Client, PropType.Generic);

				(Owner as SandboxPlayer)?.undoQueue.Add(new UndoEnt(ent));

				Tint = Color.Random.ToColor32();

				if ( !useRope )
					return;

				var rope = Particles.Create( "particles/rope.vpcf" );
				rope.SetEntity( 0, ent );

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

				var spring = PhysicsJoint.CreateLength(ent.PhysicsBody, tr.Body, 100);
				spring.SpringLinear = new( 5, 0.7f );
				spring.Collisions = true;
				spring.EnableAngularConstraint = false;
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
