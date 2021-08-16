using System.Collections.Generic;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;

namespace Sandbox.Tools
{
	[Library( "tool_gate", Title = "Gate", Description = "Spawn Gates", Group = "fun" )]
	public class ToolGate : BaseTool{
        [ConVar.ClientData("gate_selected")]
		public static string gate_selected {get; set;} = "maths_add";
        
        public override void GenerateControls(Panel inspector){
            inspector.Add.Label("Gate Type:");

            Panel GateOptions = new();
            Panel GateBox = new();
            GateOptions.AddClass("scrollable");
            inspector.AddChild(GateOptions);
            GateBox.AddClass("box");
            GateOptions.AddChild(GateBox);

            //var gateTypes = GateEntity.GateTypes();
            var gateTypes = Gate.knownGates;
            List<(string key, Button button)> buttons = new();
            foreach(var category in gateTypes){
                var CategoryBox = new Panel();
                var ChildrenBox = new Panel();
                CategoryBox.AddClass("category");
                ChildrenBox.AddClass("inactive");
                ChildrenBox.AddClass("childrenBox");
                var header = CategoryBox.Add.Button(category.Key);
                header.AddClass("Header");

                header.AddEventListener("onclick", e=>{
                    if(ChildrenBox.HasClass("inactive"))
                        ChildrenBox.RemoveClass("inactive");
                    else
                        ChildrenBox.AddClass("inactive");
                });

                CategoryBox.AddChild(ChildrenBox);
                foreach(var gate in category.Value){
                    var button = ChildrenBox.Add.Button(gate.Name);
                    buttons.Add((gate.Key, button));
                    button.SetClass("active", gate_selected == gate.Key);
                    
                    button.AddEventListener("onclick", e=>{
                        ConsoleSystem.Run("gate_selected "+gate.Key);
                        gate_selected = gate.Key;
                        foreach(var b in buttons){
                            b.button.SetClass("active", gate_selected == b.key);
                        }
                    });
                }

                GateBox.AddChild(CategoryBox);
            }

            //GateBox.Add.Button("Test");
        }

        PreviewEntity previewModel;

		private string Model => "models/citizen_props/chippacket01.vmdl";

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is GateEntity )
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

				if ( tr.Entity is GateEntity gate )
				{

					return;
				}

                var targAngle = Rotation.LookAt( tr.Normal, tr.Direction ) * Rotation.FromAxis( Vector3.Right, -90 );

				var ent = new GateEntity()
				{
					Position = tr.EndPos,
					Rotation = targAngle,
                    gateType = Owner.IsClient ? gate_selected : Owner.GetClientOwner().GetUserString("gate_selected")
				};


				if ( attached )
				{
					ent.SetParent( tr.Body.Entity, tr.Body.PhysicsGroup.GetBodyBoneName( tr.Body ) );
				}else{
                    ent.PhysicsBody.BodyType = PhysicsBodyType.Static;
                }
				
				(Owner as SandboxPlayer)?.undoQueue.Add(new UndoEnt(ent));
			}
		}

		public override string GetTitle(){
			return Gate.gatesByKey[gate_selected].Name ?? ClassInfo.Name;
		}

		public override string GetDescription(){
			return Gate.gatesByKey[gate_selected].Description ?? ClassInfo.Description;
		}
    }
}