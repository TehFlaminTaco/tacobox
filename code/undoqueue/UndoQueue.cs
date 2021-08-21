using System.Reflection.Emit;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

class UndoQueue {
    public SandboxPlayer Owner;
    public Stack<Undo> undoStack = new();

    public UndoQueue(SandboxPlayer Owner){
        this.Owner = Owner;
    }

    public bool DoUndo(){
        while (undoStack.Count > 0){
            Undo lastAction = undoStack.Pop();
            if(!lastAction.IsValid())
                continue;
            lastAction.DoUndo();
            TacoChatBox.AddChatEntry(To.Single(Owner), "white", "", lastAction.undoText(), "debug/particleerror.vtex");

            return true;
        }
        return false;
    }

    public void Add(Undo undo){
        undoStack.Push(undo);
    }
}