

public class GateRotationBreak : Gate
{
    public GateRotationBreak(){
        Key = "rotation_break";
        Name = "Break Rotation";
        Description = "Get a rotation's parts";
        Inputs = new[]{
            (WireVal.Type.Rotation, "A", "A")
        };
        Outputs = new[]{
            (WireVal.Type.Normal, "Pitch", "Pitch"),
            (WireVal.Type.Normal, "Yaw", "Yaw"),
            (WireVal.Type.Normal, "Roll", "Roll")
        };
    }

	public override void Process( GateEntity ent )
	{
        var ang = ent.GetInput<Rotation>("A").Angles();
		ent.SetOutput("Pitch", (double)ang.pitch);
		ent.SetOutput("Yaw", (double)ang.yaw);
		ent.SetOutput("Roll", (double)ang.roll);
	}
}