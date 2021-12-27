using System.Collections.Generic;
using System.IO;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[Library( "ent_texturescreen", Title = "Screen", Spawnable = true, Group = "Wire" )]
class TextureScreen : Prop, IWireEntity{
    public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/newspaper01.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
    }

    Texture target;
	Texture old;
	DrawPanel drawPanel;
	Material cameraMat;

    [Event.Frame]
	public void OnFrame(){
		if(drawPanel is null){
			drawPanel = new();
		}
		if(target is not null && target != old){
			drawPanel.screenPanel.Style.BackgroundImage = target;
			old = target;
			drawPanel.screenPanel.Style.Dirty();
		}
		drawPanel.Position = Position + Rotation.Up*3.2f*Scale - Rotation.Forward * 20f*Scale + Rotation.Right * 20f*Scale;
		drawPanel.Rotation = Rotation.RotateAroundAxis(Vector3.Up, 90f);
		drawPanel.WorldScale = Scale;

	}

	protected override void OnDestroy(){
		drawPanel?.Delete();
	}

    List<WireVal> values;
	public List<WireVal> Values()
	{
		if(values is not null)return values;
        values = new();
        values.Add(new WireValTexture("Texture", "Texture", WireVal.Direction.Input, ()=>target, r=>target=r));
        return values;
	}
}

class DrawPanel : WorldPanel{
	public Panel screenPanel;
	public DrawPanel(){
		StyleSheet.Load("/ui/RTPanel.scss");
		screenPanel = AddChild<TexPanel>();
	}
}

class TexPanel : Panel {

}