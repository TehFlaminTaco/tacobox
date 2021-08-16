

public class GateMathsSubtract : Gate
{
    public GateMathsSubtract(){
        Key = "maths_sub";
        Name = "Subtract";
        Description = "Subtract B from A";
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
		ent.SetOutput("Out", ent.GetInput<double>("A") - ent.GetInput<double>("B"));
	}
}