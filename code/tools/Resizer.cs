using System;

namespace Sandbox.Tools
{
	[Library( "tool_resizer", Title = "Resizer", Description = "Change the scale of things", Group = "construction" )]
	public partial class ResizerTool : BaseTool
	{
		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if(!this.CanTool()) return;
				int resizeDir = 0;
				var reset = false;

				if ( Input.Down( InputButton.Attack1 ) ) resizeDir = 1;
				else if ( Input.Down( InputButton.Attack2 ) ) resizeDir = -1;
				else if ( Input.Pressed( InputButton.Reload ) ) reset = true;
				else return;

				var tr = (Owner as SandboxPlayer).EyeTrace();

				if ( !tr.Hit || !tr.Entity.IsValid() || tr.Entity.PhysicsGroup == null )
					return;

				// Disable resizing lights for now
				if ( tr.Entity is LightEntity || tr.Entity is LampEntity )
					return;

				if ( !Owner.Client.CanTouch(tr.Entity) )
					return;

				var scale = reset ? 1.0f : Math.Clamp( tr.Entity.Scale + ((0.5f * Time.Delta) * resizeDir), 0.4f, 4.0f );

				if ( tr.Entity.Scale != scale )
				{
					tr.Entity.Scale = scale;
					tr.Entity.PhysicsGroup.RebuildMass();
					tr.Entity.PhysicsGroup.Sleeping = false;

					foreach ( var child in tr.Entity.Children )
					{
						if ( !child.IsValid() )
							continue;
						if(child.PhysicsGroup is not null){
							child.PhysicsGroup.RebuildMass();
							child.PhysicsGroup.Sleeping = false;
						}
						
					}
				}

				if ( Input.Pressed( InputButton.Attack1 ) || Input.Pressed( InputButton.Attack2 ) || reset )
				{
					CreateHitEffects( tr.EndPosition );
				}
			}
		}
	}
}
