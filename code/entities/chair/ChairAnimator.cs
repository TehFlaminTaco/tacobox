
namespace Sandbox
{
	public class ChairAnimator : PawnAnimator
	{
		public override void Simulate()
		{
			ResetParameters();

			SetAnimParameter( "b_grounded", true );
			SetAnimParameter( "b_sit", true );

			var eyeAngles = (Pawn.Rotation.Inverse * Pawn.EyeRotation).Angles();
			eyeAngles.pitch = eyeAngles.pitch.Clamp( -25, 70 );
			eyeAngles.yaw = eyeAngles.yaw.Clamp( -90, 90 );

			var aimPos = Pawn.EyePosition + (Pawn.Rotation * Rotation.From( eyeAngles )).Forward * 200;

			SetLookAt( "aim_eyes", aimPos );
			SetLookAt( "aim_head", aimPos );
			SetLookAt( "aim_body", aimPos );

			if ( ((Player)Pawn).ActiveChild is BaseCarriable carry )
			{
				carry.SimulateAnimator( this );
			}
			else
			{
				SetAnimParameter( "holdtype", 0 );
				SetAnimParameter( "aim_body_weight", 0.5f );
			}
		}
	}
}
