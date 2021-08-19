

public class GateRotationAroundAxis : Gate
{
    public GateRotationAroundAxis(){
        Key = "rotation_aroundaxis";
        Name = "Rotate Around Axis";
        Description = "Rotate a Rotation around an Axis";
        Inputs = new[]{
            (WireVal.Type.Rotation, "A", "A"),
            (WireVal.Type.Vector3, "Axis", "Axis"),
            (WireVal.Type.Normal, "Amount", "Amount")
        };
        Outputs = new[]{
            (WireVal.Type.Rotation, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", ent.GetInput<Rotation>("A").RotateAroundAxis(
            ent.GetInput<Vector3>("Axis"),
            (float)ent.GetInput<double>("Amount")
        ));
	}
}