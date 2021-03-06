using System.Reflection.Emit;
using Sandbox;
using Sandbox.UI;

public class TargetID : Panel {
    ITargetID lastKnown;
    public TargetID(){
        StyleSheet.Load("/ui/TargetID.scss");
    }

    public override void Tick(){
        if (Local.Pawn is not SandboxPlayer sp)return;
        var tr = sp.EyeTrace();
        
        var curTarget = tr.Entity as ITargetID;
        SetClass("hidden", curTarget is null);
        if(lastKnown != curTarget && curTarget is not null){
            if(lastKnown is not null && (lastKnown as Entity).IsValid())lastKnown.DestroyTargetID();
            DeleteChildren( true );
            lastKnown = curTarget;
            curTarget.GenerateTargetID(this);
        }
        if(curTarget is not null)curTarget.TickTargetID();
    }
}