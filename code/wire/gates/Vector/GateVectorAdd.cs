

public class GateVectorAdd : Gate
{
    public GateVectorAdd(){
        Key = "vector_add";
        Name = "Vector Add";
        Description = "Add two vectors";
        Inputs = new[]{
            (WireVal.Type.Vector3, "A", "A"),
            (WireVal.Type.Vector3, "B", "B")
        };
        Outputs = new[]{
            (WireVal.Type.Vector3, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", ent.GetInput<Vector3>("A") + ent.GetInput<Vector3>("B"));
	}
}