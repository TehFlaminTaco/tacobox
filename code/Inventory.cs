using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

partial class Inventory : BaseInventory
{
	public Inventory( Player player ) : base( player )
	{
	}

	public override bool CanAdd( Entity entity )
	{
		if ( !entity.IsValid() )
			return false;

		if ( !base.CanAdd( entity ) )
			return false;

		return !IsCarryingType( entity.GetType() );
	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		if ( !entity.IsValid() )
			return false;

		if(entity is not BaseCarriable bc)
			return false;

		if ( IsCarryingType( entity.GetType() ) )
			return false;

		return base.Add( entity, makeActive );
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x?.GetType() == t );
	}

	public override bool Drop( Entity ent )
	{
		if ( !Host.IsServer )
			return false;

		if ( !Contains( ent ) )
			return false;

		if (ent is BaseCarriable bc){
			bc.OnCarryDrop( Owner );
		}

		return ent.Parent == null;
	}

	public IEnumerable<BaseCarriable> All(int slot){
		for(int i = 0; i < Count(); i++){
			var item = GetSlot(i);
			if (item is Carriable c && c.HoldSlot == slot){
				yield return c;
			}
			if(item is Weapon w && w.HoldSlot == slot){
				yield return w;
			}
		}
	}
	public IEnumerable<BaseCarriable> All(){
		for(int i = 0; i < Count(); i++){
			var item = GetSlot(i);
			if (item is BaseCarriable c){
				yield return c;
			}
		}
	}
}
