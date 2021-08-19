

public class GateRotationRight : Gate
{
    public GateRotationRight(){
        Key = "rotation_right";
        Name = "Right";
        Description = "Get the right Vector of a Rotation";
        Inputs = new[]{
            (WireVal.Type.Rotation, "A", "A")
        };
        Outputs = new[]{
            (WireVal.Type.Vector3, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", ent.GetInput<Rotation>("A").Right);
	}
}