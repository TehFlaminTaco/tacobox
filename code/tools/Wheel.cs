using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.Tools
{
	[Library( "tool_wheel", Title = "Wheel", Description = "A wheel that you can turn on and off (but actually can't yet)", Group = "construction" )]
	public partial class WheelTool : BaseTool
	{
		[ConVar.ClientData("wheel_torque")]
		public static string wheel_torque {get; set;} = "1000.0";
		[ConVar.ClientData("wheel_maxspeed")]
		public static string wheel_maxspeed {get; set;} = "10.0";

		PreviewEntity previewModel;

		public override void GenerateControls(Panel inspector){
			inspector.Add.Label("Torque");
			var torqueEntry = inspector.Add.TextEntry(wheel_torque);
			torqueEntry.Numeric = true;
			torqueEntry.AddEventListener("onchange", e=>{
				wheel_torque = torqueEntry.Text;
				ConsoleSystem.Run("wheel_torque "+torqueEntry.Text);
			});

			inspector.Add.Label("Max Speed");
			var speedEntry = inspector.Add.TextEntry(wheel_maxspeed);
			speedEntry.Numeric = true;
			speedEntry.AddEventListener("onchange", e=>{
				wheel_maxspeed = speedEntry.Text;
				ConsoleSystem.Run("wheel_maxspeed "+speedEntry.Text);
			});

			
			Log.Info("Added");
		}

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is WheelEntity )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, "models/citizen_props/wheel01.vmdl" ) )
			{
				previewModel.RotationOffset = Rotation.FromAxis( Vector3.Up, 90 );
			}
		}

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
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

				if ( tr.Entity is WheelEntity we )
				{
					if(float.TryParse(Owner.GetClientOwner().GetUserString("wheel_torque"), out float trq)){
						we.torque=trq;
					}
					if(float.TryParse(Owner.GetClientOwner().GetUserString("wheel_maxspeed"), out float mxspd)){
						we.max_speed=mxspd;
					}

					return;
				}

				var ent = new WheelEntity
				{
					Position = tr.EndPos,
					Rotation = Rotation.LookAt( tr.Normal ) * Rotation.From( new Angles( 0, 90, 0 ) ),
				};

				ent.SetModel( "models/citizen_props/wheel01.vmdl" );

				ent.PhysicsBody.Mass = tr.Body.Mass;

				ent.Joint = PhysicsJoint.Revolute
					.From( ent.PhysicsBody )
					.To( tr.Body )
					.WithPivot( tr.EndPos )
					.WithBasis( Rotation.LookAt( tr.Normal ) * Rotation.From( new Angles( 90, 0, 0 ) ) )
					.Create();
				
				if(float.TryParse(Owner.GetClientOwner().GetUserString("wheel_torque"), out float torque)){
					ent.torque=torque;
				}
				if(float.TryParse(Owner.GetClientOwner().GetUserString("wheel_maxspeed"), out float maxspeed)){
					ent.max_speed=maxspeed;
				}

				(Owner as SandboxPlayer)?.undoQueue.Add(new UndoEnt(ent));
			}
		}
	}
}
