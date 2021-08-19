using System.Collections.Generic;
using System.IO;
using Sandbox;
using Sandbox.UI;

[Library( "ent_rtcamera", Title = "RT Camera", Spawnable = true, Group = "Wire" )]
class RTCamera : Prop, IWireEntity{
    public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/editor/camera.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
    }

    public override void ClientSpawn(){
        base.ClientSpawn();
        rt = Texture.CreateRenderTarget()
            .WithSize(1024, 1024)
            .WithDynamicUsage()
            .Create();
        depth = Texture.CreateRenderTarget()
            .WithSize(1024, 1024)
            .WithDepthFormat()
            .Create();
    }

    protected override void OnDestroy(){
        if(IsClient){
            rt.Dispose();
            depth.Dispose();
        }
    }


    Texture rt;
    Texture depth;
    [Event.Frame]
	public void OnFrame(){
        //Render.SetRenderTarget(rt);
        RenderTargets.Render.Add(()=>{
            //Render.Clear();
            Render.DrawScene(rt, depth, new Vector2(1024, 1024), SceneWorld.Current, Position + Rotation.Forward*10f, Rotation.Angles(), 90, default, default, 0.1f, 9000);
        });

        //Render.RestoreRenderTarget();
	}

    List<WireVal> values;
	public List<WireVal> Values()
	{
		if(values is not null)return values;
        values = new();
        values.Add(new WireValTexture("RenderTexture", "RenderTexture", WireVal.Direction.Output, ()=>rt, r=>rt=r));
        return values;
	}
}