using System;
using Sandbox;

public abstract class Undo {
    public abstract string undoText();
    public abstract bool IsValid();
    public abstract void DoUndo();
}

public class UndoEnt : Undo {
    public Entity ent;
    public UndoEnt(Entity ent){
        this.ent = ent;
    }

    public override string undoText() {
        if(this.ent.ToString() == "prop_physics"){
            return "Undid " + (this.ent as ModelEntity).GetModelName();
        }else{
            return "Undid " + this.ent.ToString();

        }
    }

    public override bool IsValid() {
        return ent!=null && ent.IsValid();
    }
    
    public override void DoUndo() {
        ent.Delete();
    }
}

public class UndoGeneric : Undo {
	readonly Func<bool> isValid;
	readonly Action doUndo;
	readonly string name;
    public UndoGeneric(string name, Func<bool> isValid, Action doUndo){
        this.name = name;
        this.isValid = isValid;
        this.doUndo = doUndo;
    }

    public override string undoText() {
        return name;
    }

    public override bool IsValid() {
        return isValid();
    }
    
    public override void DoUndo() {
        doUndo();
    }
}