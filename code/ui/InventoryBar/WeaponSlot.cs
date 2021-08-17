using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class WeaponSlot : Panel{
    public BaseCarriable wep;
    public WeaponSlot(BaseCarriable wep){
        SetClass("weaponslot", true);
        this.wep = wep;
        var weaponLabel = Add.Label(wep.ClassInfo.Title, "weaponname");
    }
}