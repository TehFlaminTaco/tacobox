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
        var curLeft = this.Style.Left?.GetPixels(Screen.Width)??Screen.Width / 2;
        var curTop = this.Style.Top?.GetPixels(Screen.Height)??Screen.Height;
        
        if((ply.GetActiveCamera() as Camera)?.Viewer == ply){ // First person camera active
            this.Style.Left = curLeft.LerpTo(Screen.Width / 2, 0.2f);
            this.Style.Top = curTop.LerpTo(Screen.Height / 2, 0.2f);
        }else{
            var startPos = Local.Pawn.EyePos;
            var dir = Local.Pawn.EyeRot.Forward;
            var tr = Trace.Ray( startPos, startPos + dir * 3000f )
                .Ignore( Local.Pawn )
                .Run();
            var pos = tr.EndPos.ToScreen();
            this.Style.Left = curLeft.LerpTo(pos.x * Screen.Width, 0.2f);
            this.Style.Top = curTop.LerpTo(pos.y * Screen.Height, 0.2f);
        }
        this.Style.Dirty();
    }
}