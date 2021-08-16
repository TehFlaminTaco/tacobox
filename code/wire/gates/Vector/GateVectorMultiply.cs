

public class GateVectorMultiply : Gate
{
    public GateVectorMultiply(){
        Key = "vector_mul";
        Name = "Vector Multiply";
        Description = "Multiply a Vector by a Compontent Number";
        Inputs = new[]{
            (WireVal.Type.Vector3, "A", "A"),
            (WireVal.Type.Normal, "B", "B")
        };
        Outputs = new[]{
            (WireVal.Type.Vector3, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", ent.GetInput<Vector3>("A") * (float)ent.GetInput<double>("B"));
	}
}