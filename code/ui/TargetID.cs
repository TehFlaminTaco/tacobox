using Sandbox;
using Sandbox.UI;

public class TargetID : Panel {
    ITargetID lastKnown;
    public TargetID(){
        StyleSheet.Load("/ui/TargetID.scss");
    }

    public override void Tick(){
        var startPos = Local.Pawn.EyePos;
		var dir = Local.Pawn.EyeRot.Forward;
        var tr = Trace.Ray( startPos, startPos + dir * 300f )
			.Ignore( Local.Pawn )
			.Run();
        
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