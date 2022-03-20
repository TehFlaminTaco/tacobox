using Sandbox;
using System.Linq;

[Library]
public partial class OwnerTag : Entity {
    [Net]
    public long OwnerClient {get; set;} = 0;

    [Net]
    public PropType PropType {get; set;} = 0;

    public static OwnerTag Add(Entity e){
        if(e is null || e.IsWorld || e is OwnerTag)
            return null;
        if(e.Children.OfType<OwnerTag>().FirstOrDefault() is OwnerTag o)return o;
        if(Local.Pawn is not null)return null;
        var tag = new OwnerTag();
        tag.SetParent(e);
        return tag;
    }
    public static OwnerTag GetTag(Entity e){
        if(e is null || e.IsWorld)
            return null;
        return e.Children.OfType<OwnerTag>().FirstOrDefault();
    }

    public static void Set(Entity e, long val, PropType propType){
        var tag = GetTag(e);
        if(tag is not null){
            tag.OwnerClient = val;
            tag.PropType = propType;
        }
    }

    public static long Get(Entity e){
        var tag = GetTag(e);
        if(tag is not null){
            return tag.OwnerClient;
        }
        return 0;
    }
}

public static class EntityOwnerHelper {
    public static long GetOwnerID(this Entity e){
        return OwnerTag.Get(e);
    }
    public static void SetOwnerID(this Entity e, long id, PropType propType){
        OwnerTag.Add(e).OwnerClient = id;
        OwnerTag.Add(e).PropType = propType;
    }

    public static Client GetSpawner(this Entity e){
        return Client.All.FirstOrDefault(c=>c.PlayerId == e.GetOwnerID());
    }
    public static void SetSpawner(this Entity e, Client c, PropType propType){
        SetOwnerID(e, c.PlayerId, propType);
    }
    public static void SetSpawner(this Entity e, Entity c, PropType propType){
        SetOwnerID(e, c.Client.PlayerId, propType);
    }
}