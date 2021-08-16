

public class GateMathsAdd : Gate
{
    public GateMathsAdd(){
        Key = "maths_add";
        Name = "Add";
        Description = "Add two numbers";
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
		ent.SetOutput("Out", ent.GetInput<double>("A") + ent.GetInput<double>("B"));
	}
}