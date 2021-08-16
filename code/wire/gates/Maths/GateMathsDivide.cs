

public class GateMathsDivide : Gate
{
    public GateMathsDivide(){
        Key = "maths_div";
        Name = "Divide";
        Description = "Divide A by B";
        Inputs = new[]{
            (WireVal.Type.Normal, "A", "A"),
            (WireVal.Type.Normal, "B", "B")
        };
        Outputs = new[]{
            (WireVal.Type.Normal, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", ent.GetInput<double>("A") / ent.GetInput<double>("B"));
	}
}