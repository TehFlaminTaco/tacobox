

using System.Security.Cryptography;
public class GateMathsMultiply : Gate
{
    public GateMathsMultiply(){
        Key = "maths_mul";
        Name = "Multiply";
        Description = "Multiply two numbers";
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
		ent.SetOutput("Out", ent.GetInput<double>("A") * ent.GetInput<double>("B"));
	}
}