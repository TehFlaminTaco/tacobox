using Sandbox;

[Library( "weapon_slowmopistol", Title = "Time Pistol", Spawnable = true, Group = "Weapon" )]
class SlowMoPistol : Pistol {
    public override float Damage => 100f;
    public override float Force => 30f;

    public override void AttackPrimary()
	{
        base.AttackPrimary();
        var dearSister = Sound.FromScreen("dear_sister");
        dearSister.SetVolume(0.2f);
		if(IsServer){
            ConsoleSystem.Run("host_timescale 0.1");
            Do.After(1.6f, ()=>ConsoleSystem.Run("host_timescale 1"));
        }
	}
}