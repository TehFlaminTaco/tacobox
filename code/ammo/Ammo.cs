using System.Collections.Generic;
using Sandbox;

partial class SandboxPlayer{
    //[Net, Predicted]
    //protected Dictionary<AmmoType, int> heldAmmo {get; set;} = new();

    // When dictionary support is added for [Net], I'll change this. Hopefully.
    [Net]
    public int bulletAmmo {get; set;} = 0;
    [Net]
    public int shellAmmo {get; set;} = 0;
    [Net]
    public int rocketAmmo {get; set;} = 0;

    public int AmmoCount(AmmoType typ){
        switch(typ){
            case AmmoType.None:
                return 0;
            case AmmoType.Bullet:
                return bulletAmmo;
            case AmmoType.Shell:
                return shellAmmo;
            case AmmoType.Rocket:
                return rocketAmmo;
        }
        return 0;
    }

    public void AddAmmo(AmmoType typ, int amount){
        switch(typ){
            case AmmoType.None:
                return;
            case AmmoType.Bullet:
                bulletAmmo += amount;
                return;
            case AmmoType.Shell:
                shellAmmo += amount;
                return;
            case AmmoType.Rocket:
                rocketAmmo += amount;
                return;
        }
        return;
    }

    public int RemoveAmmo(AmmoType typ, int amount){
        switch(typ){
            case AmmoType.None:
                return 0;
            case AmmoType.Bullet:
                if(bulletAmmo >= amount){
                    bulletAmmo -= amount;
                    return amount;
                }
                var t = bulletAmmo;
                bulletAmmo = 0;
                return t;
            case AmmoType.Shell:
                if(shellAmmo >= amount){
                    shellAmmo -= amount;
                    return amount;
                }
                t = shellAmmo;
                shellAmmo = 0;
                return t;
            case AmmoType.Rocket:
                if(rocketAmmo >= amount){
                    rocketAmmo -= amount;
                    return amount;
                }
                t = rocketAmmo;
                rocketAmmo = 0;
                return t;
        }
        return 0;
    }

    public bool HasAmmo(AmmoType typ, int amount = 1){
        return AmmoCount(typ)>=amount;
    }
}

partial class Weapon {
    public virtual AmmoType Clip1Type => AmmoType.None;
    public virtual AmmoType Clip2Type => AmmoType.None;
    public virtual int Clip1Size => 0;
    public virtual int Clip2Size => 0;
    public virtual bool Clip1Pocket => false;
    public virtual bool Clip2Pocket => false;
    [Net, Predicted]
    public int Clip1 {get; set;} = 0;
    [Net, Predicted]
    public int Clip2 {get; set;} = 0;
    public virtual int Ammo1PerShot => 1;
    public virtual int Ammo2PerShot => 1;
    public bool PartialReload1 => false;
    public bool PartialReload2 => false;

    public override bool CanPrimaryAttack()
    {
        if ( !Owner.IsValid() || !Input.Down( InputButton.Attack1 ) ) return false;
        if(Clip1Pocket){
            if(!((Owner as SandboxPlayer)?.HasAmmo(Clip1Type)??false)) return false;
        }else{
            if ( Clip1 <= 0 ) return false;
        }

        var rate = PrimaryRate;
        if ( rate <= 0 ) return true;

        return TimeSincePrimaryAttack > (1 / rate);
    }

    public override bool CanSecondaryAttack()
    {
        if ( !Owner.IsValid() || !Input.Down( InputButton.Attack2 ) ) return false;
        if(Clip2Pocket){
            if(!((Owner as SandboxPlayer)?.HasAmmo(Clip2Type)??false)) return false;
        }else{
            if ( Clip2 <= 0 ) return false;
        }

        var rate = SecondaryRate;
        if ( rate <= 0 ) return true;

        return TimeSinceSecondaryAttack > (1 / rate);
    }
}