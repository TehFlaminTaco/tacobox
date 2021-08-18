using Sandbox;
using Sandbox.UI;
using System.Linq;
using System.Collections.Generic;
using Sandbox.UI.Construct;

public class InventoryBar : Panel
{
	public static InventoryBar Instance;
	readonly List<InventoryIcon> slots = new();
	static float lastUpdate = Time.Now;
	Dictionary<int, WeaponGroup> groups;
	int activeGroup = 1;
	int subSlot = 1;

	public InventoryBar()
	{
		/*for ( int i = 0; i < 9; i++ )
		{
			var icon = new InventoryIcon( i + 1, this );
			slots.Add( icon );
		}*/
		ReBuild();
	}

	[Event.Hotload]
	public void Reload(){
		ReBuild();
	}
	public void ReBuild(){
		
		Instance = this;
		DeleteChildren( true );
		groups = new();
		var player = Local.Pawn;
		if ( player is not SandboxPlayer ) return;
		if ( player.Inventory is not Inventory inv ) return;

		for (int i=0; i < 9; i++){
			groups[i+1] = new WeaponGroup(i+1, this);
		}
		groups[0] = new WeaponGroup(0, this);
		knownHeld = inv.All().ToList();
	}

	public void AddItem(BaseCarriable c){
		int slot = -1;
		if (c is Weapon w){
			slot = w.HoldSlot;
		}
		if(c is Carriable car){
			slot = car.HoldSlot;
		}
		if(slot == -1)
			return;
		
		var player = Local.Pawn;
		if ( player is not SandboxPlayer sp ) return;
		if ( player.Inventory is not Inventory inv ) return;
		if(sp.ActiveChild == c){
			activeGroup = slot;
			subSlot = inv.All(activeGroup).TakeWhile(x=>x!=c).Count();
			lastUpdate = Time.Now;
		}

		groups[slot].AddWeapon(c);
	}

	public void RemoveItem(BaseCarriable c){
		int slot = -1;
		if (c is Weapon w){
			slot = w.HoldSlot;
		}
		if(c is Carriable car){
			slot = car.HoldSlot;
		}
		if(slot == -1)
			return;
		
		groups[slot].RemoveWeapon(c);
	}

	List<BaseCarriable> knownHeld = new();
	public override void Tick()
	{
		base.Tick();
		if(!groups.ContainsKey(1)){
			ReBuild();
			return;
		}

		var player = Local.Pawn;
		if ( player is not SandboxPlayer sp ) return;
		if ( player.Inventory is not Inventory inv ) return;
		SetClass("hidden", lastUpdate + 2f < Time.Now);

		var currentHeld = inv.All().ToList();

		var notYetKnown = currentHeld.Where(x => !knownHeld.Contains(x));
		var notYetRemoved = knownHeld.Where(x => !currentHeld.Contains(x));

		foreach(var wep in notYetKnown){
			AddItem(wep);
		}

		foreach(var wep in notYetRemoved){
			RemoveItem(wep);
		}

		knownHeld = currentHeld;

		foreach(var kv in groups){
			kv.Value.Update();
			kv.Value.SetClass("active", activeGroup == kv.Key);
		}

		var heldGroup = -1;
		if(sp.ActiveChild is Carriable c){
			heldGroup = c.HoldSlot;
		}
		if(sp.ActiveChild is Weapon w){
			heldGroup = w.HoldSlot;
		}

		var heldIndex = inv.All(heldGroup).TakeWhile(x=>x!=sp.ActiveChild).Count();
		if(activeGroup != heldGroup || subSlot != heldIndex){
			activeGroup = heldGroup;
			subSlot = heldIndex;
			lastUpdate = Time.Now;
		}

	}

	void EquipSelected(InputBuilder input, SandboxPlayer ply, Inventory inv){
		lastUpdate = Time.Now;
		var wep = inv.All(activeGroup).ElementAt(subSlot);
		if(ply.ActiveChild == wep)
			return;
		
		input.ActiveChild = wep;
	}

	void CycleSlot(InputBuilder input, SandboxPlayer ply, Inventory inv, int slot){
		int targetCount = inv.All(slot).Count();
		if(targetCount == 0)return;
		if(activeGroup != slot){
			subSlot = 0;
		}else{
			subSlot = subSlot + 1;
			if(subSlot >= targetCount){
				subSlot = 0;
			}
		}
		activeGroup = slot;
		EquipSelected(input, ply, inv);
	}

	void CycleDelta(InputBuilder input, SandboxPlayer ply, Inventory inv, int delta){
		if(inv.Count() == 0)return;
		subSlot = subSlot + delta;
		while(subSlot < 0){
			activeGroup--;
			if(activeGroup < 0)activeGroup = 9;
			subSlot += inv.All(activeGroup).Count();
		}
		while(subSlot >= inv.All(activeGroup).Count()){
			subSlot -= inv.All(activeGroup).Count();
			activeGroup++;
			if(activeGroup > 9)activeGroup = 0;
		}
		EquipSelected(input, ply, inv);
	}

	[Event( "buildinput" )]
	public void ProcessClientInput( InputBuilder input )
	{
		var player = Local.Pawn as SandboxPlayer;
		if ( player == null )
			return;

		var inventory = player.Inventory as Inventory;
		if ( inventory == null )
			return;

		if ( player.ActiveChild is PhysGun physgun && physgun.BeamActive )
		{
			return;
		}

		if ( input.Pressed( InputButton.Slot1 ) ) CycleSlot( input, player, inventory, 1 );
		if ( input.Pressed( InputButton.Slot2 ) ) CycleSlot( input, player, inventory, 2 );
		if ( input.Pressed( InputButton.Slot3 ) ) CycleSlot( input, player, inventory, 3 );
		if ( input.Pressed( InputButton.Slot4 ) ) CycleSlot( input, player, inventory, 4 );
		if ( input.Pressed( InputButton.Slot5 ) ) CycleSlot( input, player, inventory, 5 );
		if ( input.Pressed( InputButton.Slot6 ) ) CycleSlot( input, player, inventory, 6 );
		if ( input.Pressed( InputButton.Slot7 ) ) CycleSlot( input, player, inventory, 7 );
		if ( input.Pressed( InputButton.Slot8 ) ) CycleSlot( input, player, inventory, 8 );
		if ( input.Pressed( InputButton.Slot9 ) ) CycleSlot( input, player, inventory, 9 );
		if ( input.Pressed( InputButton.Slot0 ) ) CycleSlot( input, player, inventory, 0 );

		if ( input.MouseWheel != 0 ) CycleDelta( input, player, inventory, -input.MouseWheel );
	}

	private static void SetActiveSlot( InputBuilder input, IBaseInventory inventory, int i )
	{
		lastUpdate = Time.Now;
		var player = Local.Pawn;
		if ( player == null )
			return;

		var ent = inventory.GetSlot( i );
		if ( player.ActiveChild == ent )
			return;

		if ( ent == null )
			return;

		input.ActiveChild = ent;
	}

	private static void SwitchActiveSlot( InputBuilder input, IBaseInventory inventory, int idelta )
	{
		lastUpdate = Time.Now;
		var player = Local.Pawn;
		if ( player == null )
			return;
		var count = inventory.Count();
		if ( count == 0 ) return;
		if((player.ActiveChild as Carriable)?.OnMouseWheel(idelta)??false)
			return;

		var slot = inventory.GetActiveSlot();
		var nextSlot = slot + idelta;

		while ( nextSlot < 0 ) nextSlot += count;
		while ( nextSlot >= count ) nextSlot -= count;

		SetActiveSlot( input, inventory, nextSlot );
	}
}
