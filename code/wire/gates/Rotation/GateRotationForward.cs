

public class GateRotationForward : Gate
{
    public GateRotationForward(){
        Key = "rotation_forward";
        Name = "Forward";
        Description = "Get the forward Vector of a Rotation";
        Inputs = new[]{
            (WireVal.Type.Rotation, "A", "A")
        };
        Outputs = new[]{
            (WireVal.Type.Vector3, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", ent.GetInput<Rotation>("A").Forward);
	}
}