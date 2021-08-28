using Sandbox;
using System.Linq;

[Library]
public partial class OwnerTag : Entity {
    [Net]
    public ulong OwnerClient {get; set;} = 0;

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

    public static void Set(Entity e, ulong val, PropType propType){
        var tag = GetTag(e);
        if(tag is not null){
            tag.OwnerClient = val;
            tag.PropType = propType;
        }
    }

    public static ulong Get(Entity e){
        var tag = GetTag(e);
        if(tag is not null){
            return tag.OwnerClient;
        }
        return 0;
    }
}

public static class EntityOwnerHelper {
    public static ulong GetOwnerID(this Entity e){
        return OwnerTag.Get(e);
    }
    public static void SetOwnerID(this Entity e, ulong id, PropType propType){
        OwnerTag.Add(e).OwnerClient = id;
        OwnerTag.Add(e).PropType = propType;
    }

    public static Client GetSpawner(this Entity e){
        return Client.All.FirstOrDefault(c=>c.SteamId == e.GetOwnerID());
    }
    public static void SetSpawner(this Entity e, Client c, PropType propType){
        SetOwnerID(e, c.SteamId, propType);
    }
    public static void SetSpawner(this Entity e, Entity c, PropType propType){
        SetOwnerID(e, c.GetClientOwner().SteamId, propType);
    }
}