

public class GateVectorMagnitude : Gate
{
    public GateVectorMagnitude(){
        Key = "vector_magnitude";
        Name = "Vector Magnitude";
        Description = "Get the Magnitude of a Vector";
        Inputs = new[]{
            (WireVal.Type.Vector3, "Input", "Input")
        };
        Outputs = new[]{
            (WireVal.Type.Normal, "Magnitude", "Magnitude")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Magnitude", ent.GetInput<Vector3>("A").Length);
	}
}