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
}