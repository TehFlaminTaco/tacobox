using Sandbox;
using Sandbox.UI;

public class TacoCrosshair : Panel {
    Panel chevA;
    Panel chevB;
    Panel chevC;
    Panel chevD;
    public TacoCrosshair(){
        chevA = Add.Panel("chevA");
        chevA.AddClass("chev");
        chevB = Add.Panel("chevB");
        chevB.AddClass("chev");
        chevC = Add.Panel("chevC");
        chevC.AddClass("chev");
        chevD = Add.Panel("chevD");
        chevD.AddClass("chev");
    }
    
    public override void Tick(){
        if(Local.Pawn is not SandboxPlayer ply)
            return;
        
        if((ply.GetActiveCamera() as Camera)?.Viewer == ply){ // First person camera active
            this.Style.Left = Screen.Width / 2;
            this.Style.Top = Screen.Height / 2;
        }else{
            var startPos = Local.Pawn.EyePos;
            var dir = Local.Pawn.EyeRot.Forward;
            var tr = Trace.Ray( startPos, startPos + dir * 3000f )
                .Ignore( Local.Pawn )
                .Run();
            var pos = tr.EndPos.ToScreen();
            this.Style.Left = pos.x * Screen.Width;
            this.Style.Top = pos.y * Screen.Height;
        }
        this.Style.Dirty();
    }
}