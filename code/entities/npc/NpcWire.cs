using System.Collections.Generic;
using Sandbox;

partial class NpcTest : IWireEntity
{
    List<WireVal> values;

    public Vector3 MoveTarget = Vector3.Zero;
    public double DoMove = 0.0d;

    [Event.Tick]
    public virtual void UpdateMovement(){
        if(DoMove > 0.0d){
            Steer ??= new();
            Steer.Target = MoveTarget;
        }else{
            Steer = null;
        }
    }

	public List<WireVal> Values()
	{
        if(values is not null) return values;
        values = new();
        values.Add(new WireValVector("MoveTarget", "Move Target", WireVal.Direction.Input, ()=>MoveTarget, f=>MoveTarget = f));
        values.Add(new WireValNormal("DoMove", "Do Move", WireVal.Direction.Input, ()=>DoMove, f=>DoMove = f));
		return values;
	}
}