using System;
using Sandbox.UI;

namespace Sandbox.Tools
{
	[Library( "tool_color", Title = "Color", Description = "Change render color and alpha of entities", Group = "construction" )]
	public partial class ColorTool : BaseTool
	{
		[ConVar.ClientData("colortool_r")]
		public static string colortool_r {get; set;} = "1.0f";
		[ConVar.ClientData("colortool_g")]
		public static string colortool_g {get; set;} = "1.0f";
		[ConVar.ClientData("colortool_b")]
		public static string colortool_b {get; set;} = "1.0f";
		[ConVar.ClientData("colortool_a")]
		public static string colortool_a {get; set;} = "1.0f";

		private string ColorR => Local.Pawn is null ? Owner.Client.GetClientData("colortool_r") : colortool_r;
		private string ColorG => Local.Pawn is null ? Owner.Client.GetClientData("colortool_g") : colortool_g;
		private string ColorB => Local.Pawn is null ? Owner.Client.GetClientData("colortool_b") : colortool_b;
		private string ColorA => Local.Pawn is null ? Owner.Client.GetClientData("colortool_a") : colortool_a;


		public override void GenerateControls(Form inspector){
			var editor = new ColorEditor{
				Value = new Color(ColorR.ToFloat(), ColorG.ToFloat(), ColorB.ToFloat(), ColorA.ToFloat())
			};
			editor.AddEventListener("value.changed", e=>{
				ColorTool.colortool_r = ""+editor.Value.ToColor().r;
				ConsoleSystem.Run("colortool_r "+editor.Value.ToColor().r);
				ColorTool.colortool_g = ""+editor.Value.ToColor().g;
				ConsoleSystem.Run("colortool_g "+editor.Value.ToColor().g);
				ColorTool.colortool_b = ""+editor.Value.ToColor().b;
				ConsoleSystem.Run("colortool_b "+editor.Value.ToColor().b);
				ColorTool.colortool_a = ""+editor.Value.ToColor().a;
				ConsoleSystem.Run("colortool_a "+editor.Value.ToColor().a);
			});
			inspector.AddRow("Color", editor);
		}

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if(!this.CanTool())return;

				if ( !Input.Pressed( InputButton.Attack1 ) ) return;

				var tr = (Owner as SandboxPlayer).EyeTrace();

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				if ( tr.Entity is not ModelEntity modelEnt )
					return;
				
				if (!Owner.Client.CanTouch(tr.Entity))
					return;

				modelEnt.RenderColor = new Color(ColorR.ToFloat(), ColorG.ToFloat(), ColorB.ToFloat(), ColorA.ToFloat());

				CreateHitEffects( tr.EndPosition );
			}
		}
	}
}
