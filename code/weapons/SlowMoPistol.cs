using System;
using Sandbox;
using Sandbox.UI;

[Library( "weapon_slowmopistol", Title = "Time Pistol", Spawnable = true, Group = "Weapon" )]
class SlowMoPistol : Pistol {
    public override float Damage => 100f;
    public override float Force => 30f;

    public override void AttackPrimary()
	{
        if(IsServer)
            Global.TimeScale = 0.1f;
        base.AttackPrimary();
        var dearSister = Sound.FromScreen("dear_sister");
        dearSister.SetVolume(0.2f);
        if(IsServer){
            
            Do.After(1.6f, ()=>Global.TimeScale = 1f);
            
        }
	}
}