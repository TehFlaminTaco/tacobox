using Sandbox;

[Library( "weapon_slowmopistol", Title = "Time Pistol", Spawnable = true, Group = "Weapon" )]
class SlowMoPistol : Pistol {
    public override float Damage => 100f;
    public override float Force => 30f;

    public override void AttackPrimary()
	{
        base.AttackPrimary();
		if(IsServer){
            ConsoleSystem.Run("host_timescale 0.1");
            Do.After(1f, ()=>ConsoleSystem.Run("host_timescale 1"));
        }
	}
}