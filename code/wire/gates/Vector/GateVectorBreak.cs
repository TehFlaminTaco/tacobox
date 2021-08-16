

public class GateVectorBreak : Gate
{
    public GateVectorBreak(){
        Key = "vector_break";
        Name = "Break Vector";
        Description = "Break a Vector into X Y Z";
        Inputs = new[]{
            (WireVal.Type.Vector3, "Input", "Input")
        };
        Outputs = new[]{
            (WireVal.Type.Normal, "X", "X"),
            (WireVal.Type.Normal, "Y", "Y"),
            (WireVal.Type.Normal, "Z", "Z")
        };
    }

	public override void Process( GateEntity ent )
	{
        Vector3 inp = ent.GetInput<Vector3>("Input");
        ent.SetOutput("X", (double)inp.x);
        ent.SetOutput("Y", (double)inp.y);
        ent.SetOutput("Z", (double)inp.z);
	}
}