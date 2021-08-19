

public class GateRotationMake : Gate
{
    public GateRotationMake(){
        Key = "rotation_make";
        Name = "Make Rotation";
        Description = "Build a rotation from parts";
        Inputs = new[]{
            (WireVal.Type.Normal, "Pitch", "Pitch"),
            (WireVal.Type.Normal, "Yaw", "Yaw"),
            (WireVal.Type.Normal, "Roll", "Roll")
        };
        Outputs = new[]{
            (WireVal.Type.Rotation, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
        ent.SetOutput("Out", Rotation.From(new Angles(
            (float)ent.GetInput<double>("Pitch"),
            (float)ent.GetInput<double>("Yaw"),
            (float)ent.GetInput<double>("Roll")
        )));
	}
}