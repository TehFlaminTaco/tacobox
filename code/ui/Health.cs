using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Health : Panel
{
	public Label Label;

	public Health()
	{
		Label = Add.Label( "♥ 100", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		Label.Text = $"♥ {player.Health.CeilToInt()}";
		Label.SetClass("dying", player.Health <= 20);
		Label.SetClass("hidden", player.Health == 100);
	}
}
