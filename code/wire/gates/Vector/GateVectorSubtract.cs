

public class GateVectorSubtract : Gate
{
    public GateVectorSubtract(){
        Key = "vector_sub";
        Name = "Vector Subtract";
        Description = "Subtract B from A";
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
		ent.SetOutput("Out", ent.GetInput<Vector3>("A") - ent.GetInput<Vector3>("B"));
	}
}