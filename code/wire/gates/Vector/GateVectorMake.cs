

public class GateVectorMake : Gate
{
    public GateVectorMake(){
        Key = "vector_make";
        Name = "Make Vector";
        Description = "Build a Vector from X Y Z";
        Inputs = new[]{
            (WireVal.Type.Normal, "X", "X"),
            (WireVal.Type.Normal, "Y", "Y"),
            (WireVal.Type.Normal, "Z", "Z")
        };
        Outputs = new[]{
            (WireVal.Type.Vector3, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", new Vector3((float)ent.GetInput<double>("X"), (float)ent.GetInput<double>("Y"), (float)ent.GetInput<double>("Z")));
	}
}