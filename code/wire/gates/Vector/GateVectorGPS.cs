

using System;

public class GateVectorGPS : Gate
{
    public GateVectorGPS(){
        Key = "vector_gps";
        Name = "GPS";
        Description = "Returns its current position";
        Inputs = Array.Empty<(WireVal.Type t, string key, string name)>();
        Outputs = new[]{
            (WireVal.Type.Vector3, "Out", "Out")
        };
    }

	public override void Process( GateEntity ent )
	{
		ent.SetOutput("Out", ent.Position);
	}
}