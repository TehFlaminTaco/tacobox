using System.Collections.Generic;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;

namespace Sandbox.Tools
{
	[Library( "tool_constantvalue", Title = "Constant Value", Description = "Spawn a Constant Value", Group = "wire" )]
	public class ToolConstantValue : BaseTool{
        [ConVar.ClientData("constantvalue_value")]
		public static string constantvalue_value {get; set;} = "1.0";
        
        public override void GenerateControls(Form inspector){

			var slider = new SliderEntry{MinValue = 0, MaxValue = 10000, Value = constantvalue_value.ToFloat()};
			slider.AddEventListener("value.changed", e=>{
				constantvalue_value = ""+slider.Value;
				ConsoleSystem.Run("constantvalue_value "+slider.Value);
			});
			inspector.AddRow("Value", slider);
        }

        PreviewEntity previewModel;

		private string Model => "models/citizen_props/chippacket01.vmdl";

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is ConstantValue )
				return false;

			if ( !this.CanTool() )
				return false;

			if (!tr.Entity.IsWorld && !Owner.GetClientOwner().CanTouch(tr.Entity))
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Model ) )
			{
				//previewModel.OffsetBounds = true;
				previewModel.PositionOffset = Vector3.Zero;
                previewModel.RotationOffset = Rotation.FromAxis( Vector3.Right, -90 );
                previewModel.Scale = 0.5f;
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

				if ( !this.CanTool() ) return;

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

				if(attached && !Owner.GetClientOwner().CanTouch(tr.Entity))
					return;

				CreateHitEffects( tr.EndPos );

				if ( tr.Entity is ConstantValue gate )
				{
					gate.value = Owner.IsClient ? constantvalue_value.ToFloat() : Owner.GetClientOwner().GetUserString("constantvalue_value").ToFloat();
					return;
				}

                var targAngle = Rotation.LookAt( tr.Normal, tr.Direction ) * Rotation.FromAxis( Vector3.Right, -90 );

				if(!Owner.GetClientOwner().CanSpawn(PropType.Generic)){
					Owner.GetClientOwner().HitLimit(PropType.Generic);
					return;
				}

				var ent = new ConstantValue()
				{
					Position = tr.EndPos,
					Rotation = targAngle,
                    value = Owner.IsClient ? constantvalue_value.ToFloat() : Owner.GetClientOwner().GetUserString("constantvalue_value").ToFloat()
				};
				ent.SetSpawner(Owner.GetClientOwner(), PropType.Generic);


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