using System.Collections.Generic;
using System.Linq;
using Sandbox;

public static class PropCounts {
    public static IEnumerable<Entity> EntsOfType(this Client c, PropType type){
        return Entity.All.Where(x=>x.GetOwnerID()==c.PlayerId).Where(x=>OwnerTag.GetTag(x).PropType==type);
    }
}

public enum PropType {
    Generic,
    Prop,
    Weapon,
    NPC,
    Vehicle,
}