using System.Collections.Generic;
using Sandbox;
using System.Linq;
using System;

public abstract class WireVal{
    public string id;
    public string name;
    public abstract string TypeName {get; set;}
    public Direction direction;
    public abstract void CopyFrom(WireVal other);
    public enum Direction {
        Input,
        Output
    }
    public static WireVal FromID(Entity e, string id){
        if(e is IWireEntity we){
            return we.Values().Where(x => x.id == id).FirstOrDefault();
        }
        return null;
    }

    public enum Type{
        Normal,
        Vector3,
        Rotation,
        String,
        Texture
    }

    public static StoredWireVal FromType(Type t, string key, string name, Direction direction){
        switch(t){
            case Type.Normal:
                var normal_store=new StoredWireVal<double>();
                var normal_val = new WireValNormal(key, name, direction, ()=>normal_store.data, f=>normal_store.data=f);
                normal_store.data = 0.0d;
                normal_store.val = normal_val;
                return normal_store;
            case Type.Vector3:
                var vec3_store=new StoredWireVal<Vector3>();
                var vec3_val = new WireValVector(key, name, direction, ()=>vec3_store.data, f=>vec3_store.data=f);
                vec3_store.data = new Vector3();
                vec3_store.val = vec3_val;
                return vec3_store;
            case Type.Rotation:
                var rot_store=new StoredWireVal<Rotation>();
                var rot_val = new WireValRotation(key, name, direction, ()=>rot_store.data, f=>rot_store.data=f);
                rot_store.data = Rotation.Identity;
                rot_store.val = rot_val;
                return rot_store;
            case Type.String:
                var str_store=new StoredWireVal<string>();
                var str_val = new WireValString(key, name, direction, ()=>str_store.data, f=>str_store.data=f);
                str_store.data = string.Empty;
                str_store.val = str_val;
                return str_store;
            case Type.Texture:
                var tex_store=new StoredWireVal<Texture>();
                var tex_val = new WireValTexture(key, name, direction, ()=>tex_store.data, f=>tex_store.data=f);
                tex_store.data = null;
                tex_store.val = tex_val;
                return tex_store;
        }
        return null;
    }
}

public abstract class StoredWireVal {
    public WireVal val;
}

public class StoredWireVal<T> : StoredWireVal {
    public T data;
}

public abstract class WireVal<T> : WireVal{
    public Func<T> getter;
    public Action<T> setter;
    public WireVal(string id, string name, Direction direction, Func<T> getter, Action<T> setter){
        this.id = id;
        this.name = name;
        this.direction = direction;
        this.getter = getter;
        this.setter = setter;
    }
}

public class WireValNormal : WireVal<double> {
    public override string TypeName {get; set;} = "Normal";
    public WireValNormal(string id, string name, Direction direction, Func<double> getter, Action<double> setter) : base(id, name, direction, getter, setter){}
	public override void CopyFrom( WireVal other ){
		if(other is WireValNormal n){
            setter(n.getter());
        }
        
        if(other is WireValString s){
            setter(s.getter().ToFloat());
        }
	}
}

public class WireValVector : WireVal<Vector3> {
    public override string TypeName {get; set;} = "Vector";
    public WireValVector(string id, string name, Direction direction, Func<Vector3> getter, Action<Vector3> setter) : base(id, name, direction, getter, setter){}
	public override void CopyFrom( WireVal other ){
		if(other is WireValVector n){
            setter(n.getter());
            return;
        }
	}
}

public class WireValString : WireVal<string> {
    public override string TypeName {get; set;} = "String";
    public WireValString(string id, string name, Direction direction, Func<string> getter, Action<string> setter) : base(id, name, direction, getter, setter){}
	public override void CopyFrom( WireVal other ){
		if(other is WireValNormal n){
            setter(n.getter().ToString());
            return;
        }
        if(other is WireValVector v){
            setter(v.getter().ToString());
            return;
        }
        if(other is WireValRotation r){
            setter(r.getter().ToString());
            return;
        }
        if(other is WireValString s){
            setter(s.getter());
            return;
        }
	}
}

public class WireValRotation : WireVal<Rotation> {
    public override string TypeName {get; set;} = "Rotation";
    public WireValRotation(string id, string name, Direction direction, Func<Rotation> getter, Action<Rotation> setter) : base(id, name, direction, getter, setter){}
	public override void CopyFrom( WireVal other ){
		if(other is WireValRotation t){
            setter(t.getter());
            return;
        }
        return;
	}
}

public class WireValTexture : WireVal<Texture> {
    public override string TypeName {get; set;} = "Texture";
    public WireValTexture(string id, string name, Direction direction, Func<Texture> getter, Action<Texture> setter) : base(id, name, direction, getter, setter){}
	public override void CopyFrom( WireVal other ){
		if(other is WireValTexture t){
            setter(t.getter());
            return;
        }
        return;
	}
}