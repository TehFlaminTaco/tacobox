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

		public override void GenerateControls(Form inspector){
			var slider = new SliderEntry{MinValue = 0, MaxValue = 2000, Value = wheel_torque.ToFloat()};
			slider.AddEventListener("value.changed", e=>{
				wheel_torque = ""+slider.Value;
				ConsoleSystem.Run("wheel_torque "+slider.Value);
			});
			inspector.AddRow("Torque", slider);

			slider = new SliderEntry{MinValue = 0, MaxValue = 200, Value = wheel_maxspeed.ToFloat()};
			slider.AddEventListener("value.changed", e=>{
				wheel_maxspeed = ""+slider.Value;
				ConsoleSystem.Run("wheel_maxspeed "+slider.Value);
			});
			inspector.AddRow("Max Speed", slider);
		}

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is WheelEntity )
				return false;

			if ( !this.CanTool() )
				return false;

			if (!tr.Entity.IsWorld && !Owner.Client.CanTouch(tr.Entity))
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

				if( !this.CanTool() )
					return;

				var tr = (Owner as SandboxPlayer).EyeTrace();

				if ( !tr.Hit )
					return;

				if ( !tr.Entity.IsValid() )
					return;

				var attached = !tr.Entity.IsWorld && tr.Body.IsValid() && tr.Body.PhysicsGroup != null && tr.Body.GetEntity().IsValid();

				if ( attached && tr.Entity is not Prop )
					return;

				if(attached && !Owner.Client.CanTouch(tr.Entity))
					return;

				CreateHitEffects( tr.EndPosition );

				if ( tr.Entity is WheelEntity we )
				{
					if(float.TryParse(Owner.Client.GetClientData("wheel_torque"), out float trq)){
						we.torque=trq;
					}
					if(float.TryParse(Owner.Client.GetClientData("wheel_maxspeed"), out float mxspd)){
						we.max_speed=mxspd;
					}

					return;
				}

				if(!Owner.Client.CanSpawnProp("citizen_props/wheel01.vmdl")){
					Owner.Client.BannedProp("models/citizen_props/wheel01.vmdl");
					return;
				}

				if(!Owner.Client.CanSpawn(PropType.Generic)){
					Owner.Client.HitLimit(PropType.Generic);
					return;
				}

				var ent = new WheelEntity
				{
					Position = tr.EndPosition,
					Rotation = Rotation.LookAt( tr.Normal ) * Rotation.From( new Angles( 0, 90, 0 ) ),
				};
				ent.SetSpawner(Owner.Client, PropType.Generic);

				ent.SetModel( "models/citizen_props/wheel01.vmdl" );

				ent.PhysicsBody.Mass = tr.Body.Mass;

				ent.Joint = PhysicsJoint.CreateHinge( ent.PhysicsBody, tr.Body, tr.EndPosition, tr.Normal );
				
				if(float.TryParse(Owner.Client.GetClientData("wheel_torque"), out float torque)){
					ent.torque=torque;
				}
				if(float.TryParse(Owner.Client.GetClientData("wheel_maxspeed"), out float maxspeed)){
					ent.max_speed=maxspeed;
				}

				(Owner as SandboxPlayer)?.undoQueue.Add(new UndoEnt(ent));
			}
		}
	}
}
