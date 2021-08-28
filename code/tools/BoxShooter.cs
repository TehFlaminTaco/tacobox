namespace Sandbox.Tools
{
	[Library( "tool_boxgun", Title = "Box Shooter", Description = "Shoot boxes", Group = "fun" )]
	public class BoxShooter : BaseTool
	{
		TimeSince timeSinceShoot;

		public override void Simulate()
		{
			if ( Host.IsServer )
			{
				if(!this.CanTool())return;

				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					ShootBox();
				}

				if ( Input.Down( InputButton.Attack2 ) && timeSinceShoot > 0.05f )
				{
					timeSinceShoot = 0;
					ShootBox();
				}
			}
		}

		void ShootBox()
		{
			if(!Owner.GetClientOwner().CanSpawn(PropType.Prop)){
				Owner.GetClientOwner().HitLimit(PropType.Prop);
				return;
			}

			var ent = new Prop
			{
				Position = Owner.EyePos + Owner.EyeRot.Forward * 50,
				Rotation = Owner.EyeRot
			};

			ent.SetSpawner(Owner.GetClientOwner(), PropType.Prop);
			ent.SetModel( "models/citizen_props/crate01.vmdl" );
			ent.Velocity = Owner.EyeRot.Forward * 1000;
			(Owner as SandboxPlayer)?.undoQueue.Add(new UndoEnt(ent));
		}
	}
}
