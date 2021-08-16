using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class AmmoHud : Panel
{
	public Label clip1Text;
	public Label clip2Text;
	public Label held1Text;
	public Label held2Text;

	public AmmoHud()
	{
		clip1Text = Add.Label( "0", "clip one" );
		clip2Text = Add.Label( "0", "clip two" );
		held1Text = Add.Label( "0", "held one" );
		held2Text = Add.Label( "0", "held two" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as SandboxPlayer;
		if ( player == null ) return;

		var ActiveWeapon = player.Inventory.Active as Weapon;

		if(ActiveWeapon is null){
			clip1Text.SetClass("hidden", true);
			clip2Text.SetClass("hidden", true);
			held1Text.SetClass("hidden", true);
			held2Text.SetClass("hidden", true);
			return;
		}

		/*Label.Text = $"♥ {player.Health.CeilToInt()}";
		Label.SetClass("dying", player.Health <= 20);
		Label.SetClass("hidden", player.Health == 100);*/

		//clip1Text.SetClass("hidden")
		clip1Text.SetClass("hidden", ActiveWeapon.Clip1Type == AmmoType.None);
		clip2Text.SetClass("hidden", ActiveWeapon.Clip2Type == AmmoType.None);
		held1Text.SetClass("hidden", ActiveWeapon.Clip1Type == AmmoType.None);
		held2Text.SetClass("hidden", ActiveWeapon.Clip2Type == AmmoType.None);

		clip1Text.Text = $"{ActiveWeapon.Clip1}";
		clip2Text.Text = $"{ActiveWeapon.Clip2}";

		held1Text.Text = $"{player.AmmoCount(ActiveWeapon.Clip1Type)}".PadLeft(3, '0');
		held2Text.Text = $"{player.AmmoCount(ActiveWeapon.Clip2Type)}".PadLeft(3, '0');
	}
}
