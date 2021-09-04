namespace Sandbox.Tools
{
	[Library( "tool_remover", Title = "Remover", Description = "Remove entities", Group = "construction" )]
	public partial class RemoverTool : BaseTool
	{
		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;
			if(!this.CanTool())return;
			using ( Prediction.Off() )
			{
				if ( !Input.Pressed( InputButton.Attack1 ) )
					return;

				var tr = (Owner as SandboxPlayer).EyeTrace();

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				if ( tr.Entity is Player )
					return;

				if ( !Owner.GetClientOwner().CanTouch(tr.Entity) )
					return;

				CreateHitEffects( tr.EndPos );

				if ( tr.Entity.IsWorld )
					return;

				tr.Entity.Delete();

				var particle = Particles.Create( "particles/physgun_freeze.vpcf" );
				particle.SetPosition( 0, tr.Entity.Position );
			}
		}
	}
}
