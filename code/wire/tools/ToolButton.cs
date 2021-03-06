using System.Collections.Generic;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;

namespace Sandbox.Tools
{
	[Library( "tool_wirebutton", Title = "Button", Description = "Spawn a Button", Group = "wire" )]
	public class ToolButton : BaseTool{
        [ConVar.ClientData("button_lowvalue")]
		public static string button_lowvalue {get; set;} = "0.0";
		[ConVar.ClientData("button_highvalue")]
		public static string button_highvalue {get; set;} = "1.0";
		[ConVar.ClientData("button_toggle")]
		public static string button_toggle {get; set;} = "false";
        
        public override void GenerateControls(Form inspector){
			var slider = new SliderEntry{MinValue = 0, MaxValue = 10000, Value = button_lowvalue.ToFloat()};
			slider.AddEventListener("value.changed", e=>{
				button_lowvalue = ""+slider.Value;
				ConsoleSystem.Run("button_lowvalue "+slider.Value);
			});
			inspector.AddRow("Value Off", slider);
			var slider2 = new SliderEntry{MinValue = 0, MaxValue = 10000, Value = button_highvalue.ToFloat()};
			slider2.AddEventListener("value.changed", e=>{
				button_highvalue = ""+slider2.Value;
				ConsoleSystem.Run("button_highvalue "+slider2.Value);
			});
			inspector.AddRow("Value On", slider2);
			var checkbox = new Checkbox{
				LabelText = "Toggleable",
				Checked = button_toggle.ToBool()
			};
			checkbox.AddEventListener("onchange", e=>{
				button_toggle = ""+checkbox.Checked;
				ConsoleSystem.Run("button_toggle "+checkbox.Checked);
			});
			inspector.AddChild(checkbox);
        }

        PreviewEntity previewModel;

		private string Model => "models/wirebox/katlatze/button.vmdl";

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is WireButton )
				return false;

			if ( !this.CanTool() )
				return false;

			if (!tr.Entity.IsWorld && !Owner.Client.CanTouch(tr.Entity))
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
				if ( !this.CanTool() )
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

				if ( tr.Entity is WireButton gate )
				{
					//gate.value = Owner.IsClient ? constantvalue_value.ToFloat() : Owner.Client.GetClientData("constantvalue_value").ToFloat();
					return;
				}

                var targAngle = Rotation.LookAt( tr.Normal, tr.Direction ) * Rotation.FromAxis( Vector3.Right, -90 );

				if(!Owner.Client.CanSpawnProp("wirebox/katlatze/button.vmdl")){
					Owner.Client.BannedProp("models/wirebox/katlatze/button.vmdl");
					return;
				}

				if(!Owner.Client.CanSpawn(PropType.Generic)){
					Owner.Client.HitLimit(PropType.Generic);
					return;
				}

				var ent = new WireButton()
				{
					Position = tr.EndPosition,
					Rotation = targAngle,
                    low_value = Owner.IsClient ? button_lowvalue.ToFloat() : Owner.Client.GetClientData("button_lowvalue").ToFloat(),
                    high_value = Owner.IsClient ? button_highvalue.ToFloat() : Owner.Client.GetClientData("button_highvalue").ToFloat(),
					toggleable = Owner.IsClient ? button_toggle.ToBool() : Owner.Client.GetClientData("button_toggle").ToBool()
				};
				ent.SetSpawner(Owner.Client, PropType.Generic);


				if ( attached )
				{
					ent.SetParent( tr.Body.GetEntity(), tr.Body.GroupName );
				}else{
                    ent.PhysicsBody.BodyType = PhysicsBodyType.Static;
                }
				
				(Owner as SandboxPlayer)?.undoQueue.Add(new UndoEnt(ent));
			}
		}
    }
}