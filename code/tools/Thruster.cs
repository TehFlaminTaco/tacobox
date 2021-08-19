using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.Tools
{

	[Library( "tool_thruster", Title = "Thruster", Description = "A rocket type thing that can push forwards and backward", Group = "construction" )]
	public partial class ThrusterTool : BaseTool
	{
		public static string[] Models = new[]{
			"models/thruster/thrusterprojector.vmdl",
			"models/citizen_props/sodacan01.vmdl"
		};

		[ConVar.ClientData("thruster_force")]
		public static string thrusterForce {get; set;} = "5000.0";
		[ConVar.ClientData("thruster_model")]
		public static string thruster_model {get; set;} = "models/thruster/thrusterprojector.vmdl";
		private string Model => Local.Pawn is null ? Owner.GetClientOwner().GetUserString("thruster_model") : thruster_model;

		PreviewEntity previewModel;
		bool massless = true;

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Model ) )
			{
				previewModel.RotationOffset = Rotation.FromAxis( Vector3.Right, -90 );
			}
		}

		public override void GenerateControls(Form inspector){
			var slider = new SliderEntry{MinValue = 0, MaxValue = 10000, Value = thrusterForce.ToFloat()};
			slider.AddEventListener("value.changed", e=>{
				ThrusterTool.thrusterForce = ""+slider.Value;
				ConsoleSystem.Run("thruster_force "+slider.Value);
			});
			inspector.AddRow("Thrust", slider);

			var picker = new ModelPicker(Models, ()=>thruster_model, s=>{
				thruster_model=s;
				ConsoleSystem.Run("thruster_model "+s);
			});

			inspector.AddRow("Model", picker);
		}

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is ThrusterEntity )
				return false;

			return true;
		}

		public override void Simulate()
		{
			if(previewModel is not null && previewModel.GetModelName() != Model)
				previewModel.SetModel(Model);

			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.Attack2 ) )
				{
					massless = !massless;
				}

				if ( !Input.Pressed( InputButton.Attack1 ) )
					return;

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit )
					return;

				if ( !tr.Entity.IsValid() )
					return;

				var attached = !tr.Entity.IsWorld && tr.Body.IsValid() && tr.Body.PhysicsGroup != null && tr.Body.Entity.IsValid();

				if ( attached && tr.Entity is not Prop )
					return;

				CreateHitEffects( tr.EndPos );

				if ( tr.Entity is ThrusterEntity te )
				{
					if(float.TryParse(Owner.GetClientOwner().GetUserString("thruster_force"), out float frc)){
						te.Force = frc;
					}
					return;
				}

				var ent = new ThrusterEntity
				{
					Position = tr.EndPos,
					Rotation = Rotation.LookAt( tr.Normal, dir ) * Rotation.From( new Angles( 90, 0, 0 ) ),
					PhysicsEnabled = !attached,
					EnableSolidCollisions = !attached,
					TargetBody = attached ? tr.Body : null,
					Massless = massless
				};

				ent.SetModel(Model);
				ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );


				if(float.TryParse(Owner.GetClientOwner().GetUserString("thruster_force"), out float force)){
					ent.Force = force;
				}

				if ( attached )
				{
					ent.SetParent( tr.Body.Entity, tr.Body.PhysicsGroup.GetBodyBoneName( tr.Body ) );
				}else{
                    ent.PhysicsBody.BodyType = PhysicsBodyType.Static;
                }

				(Owner as SandboxPlayer)?.undoQueue.Add(new UndoEnt(ent));
			}
		}
	}
}
