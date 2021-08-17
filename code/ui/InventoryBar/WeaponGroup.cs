
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class WeaponGroup : Panel {
    public List<(BaseCarriable weapon, WeaponSlot panel)> slots;
    public WeaponGroup(int index){
        var player = Local.Pawn;
		if ( player == null ) return;
		if ( player.Inventory == null ) return;
        slots = new();

        AddClass("group");
		var groupHeader = Add.Panel("groupHeader");
		
		foreach(var wep in (player.Inventory as Inventory).All(index)){
			AddWeapon(wep);
		}
		var groupText = Add.Label($"{index}", "groupName");
    }

    public WeaponGroup(int index, Panel parent) : this(index){
        parent.AddChild(this);
    }

    public void AddWeapon(BaseCarriable wep){
        if(slots.Where(x=>x.weapon == wep).Any()) return;
        var weaponSlot = new WeaponSlot(wep);
		AddChild(weaponSlot);
        slots.Add((wep, weaponSlot));
    }

    public void RemoveWeapon(BaseCarriable wep){
        foreach(var slot in slots.Where(x=>x.weapon == wep)){
            slot.panel.Delete();
        }
        slots.RemoveAll(x=>x.weapon == wep);
    }

    public void Update(){
        var player = Local.Pawn;
		if ( player == null ) return;
		if ( player.Inventory == null ) return;
        foreach(var slot in slots){
            slot.panel.SetClass("active", slot.weapon == player.Inventory.Active);
        }
    }
}