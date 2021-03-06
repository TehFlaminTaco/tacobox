namespace Sandbox.Tools
{
	[Library( "no_collide", Title = "No Collide", Description = "Remove collision between two objects", Group = "construction" )]
	public partial class NoCollide : BaseTool
	{
		private Prop target;

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;
			if(!this.CanTool())return;
			using ( Prediction.Off() )
			{
				var tr = (Owner as SandboxPlayer).EyeTrace();

				if ( !tr.Hit || !tr.Body.IsValid() || !tr.Entity.IsValid() || tr.Entity.IsWorld )
					return;

				if ( tr.Entity.PhysicsGroup == null || tr.Entity.PhysicsGroup.BodyCount > 1 )
					return;

				if ( tr.Entity is not Prop prop )
					return;

				if ( !Owner.Client.CanTouch(tr.Entity) )
					return;

				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					if ( prop.Root is not Prop rootProp )
					{
						return;
					}

					if ( target == rootProp )
						return;

					if ( !target.IsValid() )
					{
						target = rootProp;
					}
					else
					{
                        var joint = PhysicsJoint.CreateLength(target.PhysicsBody, rootProp.PhysicsBody, float.MaxValue);
                        joint.Collisions = false;
                        (Owner as SandboxPlayer)?.undoQueue.Add(new UndoGeneric("Undid No Collide", ()=>{
                            return joint.IsValid();
                        }, ()=>{
                            joint.Remove();
                        }));
						//target.Weld( rootProp );
						target = null;
					}
				}
				else if ( Input.Pressed( InputButton.Attack2 ) )
				{
					//prop.Unweld( true );

					Reset();
				}
				else if ( Input.Pressed( InputButton.Reload ) )
				{
					if ( prop.Root is not Prop rootProp )
					{
						return;
					}

					//rootProp.Unweld();

					Reset();
				}
				else
				{
					return;
				}

				CreateHitEffects( tr.EndPosition );
			}
		}

		private void Reset()
		{
			target = null;
		}

		public override void Activate()
		{
			base.Activate();

			Reset();
		}

		public override void Deactivate()
		{
			base.Deactivate();

			Reset();
		}
	}
}
