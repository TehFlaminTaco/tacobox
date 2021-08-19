

public class GateRotationUp : Gate
{
    public GateRotationUp(){
        Key = "rotation_up";
        Name = "Up";
        Description = "Get the up Vector of a Rotation";
        Inputs = new[]{
            (WireVal.Type.Rotation, "A", "A")
        };
        Outputs = new[]{
            (WireVal.Type.Vector3, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", ent.GetInput<Rotation>("A").Up);
	}
}