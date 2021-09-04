using System.Collections.Generic;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;

namespace Sandbox.Tools
{
	[Library( "tool_gate", Title = "Gate", Description = "Spawn Gates", Group = "wire" )]
	public class ToolGate : BaseTool{
		public static string[] Models = new[]{
			"models/wirebox/katlatze/chip_rectangle.vmdl",
			"models/wirebox/katlatze/chip_rectangle_logo.vmdl",
			"models/wirebox/katlatze/chip_square.vmdl",
			"models/wirebox/katlatze/constant_value.vmdl",
			"models/wirebox/katlatze/e2.vmdl"
		};

        [ConVar.ClientData("gate_selected")]
		public static string gate_selected {get; set;} = "maths_add";

		[ConVar.ClientData("gate_model")]
		public static string gate_model {get; set;} = "models/wirebox/katlatze/chip_square.vmdl";
        
        public override void GenerateControls(Form inspector){
            Panel GateOptions = new();
            Panel GateBox = new();
			Form subForm = new Form();
            GateOptions.AddClass("scrollable");
            inspector.AddRow("Gate Type", GateOptions);
            GateBox.AddClass("box");
            GateOptions.AddChild(GateBox);

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
						if(Gate.gatesByKey.ContainsKey(gate_selected)){
							Gate.gatesByKey[gate_selected].DestroyControls();
						}
						subForm.DeleteChildren(true);
                        gate_selected = gate.Key;
						if(Gate.gatesByKey.ContainsKey(gate_selected)){
							Gate.gatesByKey[gate_selected].GenerateControls(subForm);
						}
                        foreach(var b in buttons){
                            b.button.SetClass("active", gate_selected == b.key);
                        }
                    });
                }

                GateBox.AddChild(CategoryBox);
            }
			var picker = new ModelPicker(Models, ()=>gate_model, s=>{
				gate_model=s;
				ConsoleSystem.Run("gate_model "+s);
			});

			inspector.AddRow("Model", picker);
			inspector.AddChild(subForm);

			if(Gate.gatesByKey.ContainsKey(gate_selected)){
            	Gate.gatesByKey[gate_selected].GenerateControls(subForm);
			}
        }

        PreviewEntity previewModel;

		private string Model => Local.Pawn is null ? Owner.GetClientOwner().GetClientData("gate_model") : gate_model;

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is GateEntity )
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
			if(previewModel is not null && previewModel.GetModelName() != Model)
				previewModel.SetModel(Model);
			

			if ( !Host.IsServer ){
				if(Gate.gatesByKey.ContainsKey(gate_selected))
					Gate.gatesByKey[gate_selected].TickControls();
				return;
			}

			using ( Prediction.Off() )
			{
				if ( !Input.Pressed( InputButton.Attack1 ) )
					return;

				if ( !this.CanTool() ) return;

				var tr = (Owner as SandboxPlayer).EyeTrace();

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

				if ( tr.Entity is GateEntity gate )
				{

					return;
				}

				if(!Owner.GetClientOwner().CanSpawnProp(Model.Substring(7))){
					Owner.GetClientOwner().BannedProp(Model);
					return;
				}
				if(!Owner.GetClientOwner().CanSpawn(PropType.Generic)){
					Owner.GetClientOwner().HitLimit(PropType.Generic);
					return;
				}

                var targAngle = Rotation.LookAt( tr.Normal, tr.Direction ) * Rotation.FromAxis( Vector3.Right, -90 );

				var targetGate = Owner.IsClient ? gate_selected : Owner.GetClientOwner().GetClientData("gate_selected");
				var ent = new GateEntity()
				{
					Position = tr.EndPos,
					Rotation = targAngle,
                    gateType = targetGate
				};
				ent.GateSpawner = (Local.Pawn as Player) ?? Owner;
				//ent.values = Gate.gatesByKey[targetGate].GenerateValues(ent.GateSpawner, ent);
				ent.SetModel(Model);
				ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
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

		public override string GetTitle(){
			return Gate.gatesByKey[gate_selected].Name ?? ClassInfo.Name;
		}

		public override string GetDescription(){
			return Gate.gatesByKey[gate_selected].Description ?? ClassInfo.Description;
		}
    }
}