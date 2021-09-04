using Sandbox;
using Sandbox.UI;

public class TargetID : Panel {
    ITargetID lastKnown;
    public TargetID(){
        StyleSheet.Load("/ui/TargetID.scss");
    }

    public override void Tick(){
        var tr = (Local.Pawn as SandboxPlayer).EyeTrace();
        
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