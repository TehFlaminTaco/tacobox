using System.Text;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

class WireHUD : HudEntity<RootPanel> {
    public static bool HoveredPanel = false;
    public static Entity HoveredEntity = null;
    public static string HoveredKey = null;
    public static bool DidHover = false;
    public static WireVal HoveredPair;
    public static WireGun gun;
    public WireHUD(WireGun gun){
        if(!IsClient)
            return;
        
        RootPanel.StyleSheet.Load("/ui/WireHud.scss");
        RootPanel.AddChild<WirePanel>();
        WireHUD.gun = gun;
    }
}

class WirePanel : Panel {
    public WirePanel(){
    }

    Dictionary<IWireEntity, WireEntityPanel> knownEnts = new();

    public override void Tick(){
        WireHUD.HoveredPanel = false;
        WireHUD.HoveredEntity = null;
        WireHUD.HoveredKey = null;
        if(!WireHUD.DidHover){
            WireHUD.HoveredPair = null;
        }
        WireHUD.DidHover = false;
        var ents = Entity.All.OfType<IWireEntity>().ToList();

        var newEnts = ents.Where(x=>!knownEnts.ContainsKey(x));
        var removeEnts = knownEnts.Where(x=>!(x.Key as Entity).IsValid()||!ents.Contains(x.Key)).Select(x=>x.Key);

        foreach(var ent in removeEnts){
            knownEnts[ent].Delete(true);
            knownEnts.Remove(ent);
        }
        foreach(var ent in newEnts){
            AddChild(knownEnts[ent] = new(ent));
        }

        foreach(var kv in knownEnts.OrderBy(x=>(x.Key as Entity).Position.Distance(Local.Pawn.EyePos))){
            kv.Value.Tick();
        }

    }
}

class WireEntityPanel : Panel {
    IWireEntity target;
    Label Name;
    Panel IO;
    List<WireIOPanel> ioDevices = new();
    public WireEntityPanel(IWireEntity target){
        this.target = target;
        Name = Add.Label((target as Entity).ClassInfo.Title, "entname");
        IO = Add.Panel( "io" );
        var inputs = IO.Add.Panel("value inputs");
        var outputs = IO.Add.Panel("value outputs");

        foreach(var val in target.Values()){
            if(val.direction == WireVal.Direction.Input){
                var panel = new WireInPanel(target as Entity, val);
                inputs.AddChild(panel);
                ioDevices.Add(panel);
            }else{
                var panel = new WireOutPanel(target as Entity, val);
                outputs.AddChild(panel);
                ioDevices.Add(panel);
            }
        }
    }

    public override void Tick(){
        if(target is not Entity e)return;
        if(e.Parent is Player){
            Style.Opacity = 0.0f;
            Style.Dirty();
            return;
        }

        var pos = e.Position.ToScreen();
        this.Style.ZIndex = (int)-e.Position.Distance(Local.Pawn.EyePos)*300;

        Style.Left = Length.Pixels(pos.x * Screen.Width);
        Style.Top = Length.Pixels(pos.y * Screen.Height);


        foreach(var p in ioDevices){
            p.Tick();
        }

        var inX = IO.Box.Left <= Screen.Width / 2 && IO.Box.Right > Screen.Width / 2;
        var inY = IO.Box.Top <= Screen.Height / 2 && IO.Box.Bottom > Screen.Height / 2;
        SetClass("targeted", inX && inY && !WireHUD.HoveredPanel);
        if(inX && inY && !WireHUD.HoveredPanel){
            WireHUD.HoveredPanel = true;
            this.Style.ZIndex = 9000;
        }
        Style.Dirty();
    }
}

class WireIOPanel : Panel {
    public Label name;
    public Label type;
    public Panel box;
    public WireVal val;
    public Entity ent;
    public Panel connectionBox;
    public Label connected;
    public Label connectedEnt;
    public Label connectedKey;
    public Panel connection;

    public WireIOPanel(Entity ent, WireVal val){
        AddClass("iopanel");
        this.val = val;
        this.ent = ent;
        connectionBox = new Panel();
        connectionBox.AddClass("connectionBox");
        connected = new Label();
        connected.AddClass("connectedArrow");
        connected.Text = " -> ";
        connection = new();
        connectedEnt = connection.Add.Label("", "targetEntity");
        connectedKey = connection.Add.Label("", "targetName");
        box = new();
        type = box.Add.Label(val.TypeName, "type");
        name = box.Add.Label(val.name, "name");
    }

    public override void Tick(){


        var inX = Box.Left <= Screen.Width / 2 && Box.Right > Screen.Width / 2;
        var inY = Box.Top <= Screen.Height / 2 && Box.Bottom > Screen.Height / 2;

        var hoveredOver = !WireHUD.HoveredPanel && inX && inY;
        bool targetingOutput = WireHUD.gun.selectedEntity is not null && WireHUD.gun.selectedEntity.IsValid();
        hoveredOver &= targetingOutput ^ val.direction==WireVal.Direction.Input;
        SetClass("active", hoveredOver);
        if(hoveredOver){
            WireHUD.HoveredEntity = ent;
            WireHUD.HoveredKey = val.id;
        }
        SetClass("hoverConnection", WireHUD.HoveredPair == val);
        SetClass("selected", WireHUD.gun.selectedEntity == ent && WireHUD.gun.selectedID == val.id);

        Entity ConnTarget = null;
        string ConnName = null;

        var conn = WireConnection.GetConnection(ent, val.id);
        if(conn.target is not null && conn.target.IsValid()){
            connectionBox.SetClass("hidden", false);
            connectedEnt.Text = conn.target.ClassInfo.Title;
            var targ = (conn.target as IWireEntity).Values().Where(x=>x.id==conn.id).First();
            if(hoveredOver){
                WireHUD.DidHover = true;
                WireHUD.HoveredPair = targ;
            }
            connectedKey.Text = targ.name;
        }else{
            connectionBox.SetClass("hidden", true);
        }
    }
}

class WireInPanel : WireIOPanel {
    public WireInPanel(Entity ent, WireVal val) : base(ent, val){
        AddChild(connectionBox);
        connectionBox.AddChild(connection);
        connectionBox.AddChild(connected);
        AddChild(box);
    }
}

class WireOutPanel : WireIOPanel {
    public WireOutPanel(Entity ent, WireVal val) : base(ent, val){
        AddChild(box);
    }
}

/*
class WirePanelOld : Panel {
    public Label Label;
    public WireGun gun;

	public WirePanelOld()
	{
		Label = Add.Label( "", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

        StringBuilder sb = new StringBuilder();
        if(gun.wireTarget is IWireEntity ent){
            IEnumerable<WireVal> vals = ent.Values();
            if(gun.selectedEntity is null){
                sb.Append("Inputs:");
                vals = vals.Where(x => x.direction==WireVal.Direction.Input);
            }else{
                sb.Append("Outputs:");
                vals = vals.Where(x => x.direction==WireVal.Direction.Output);
            }
            int i = 0;
            foreach(var output in vals){
                sb.Append('\n');
                sb.Append(output.TypeName);
                sb.Append('\t');
                if(i++ == WireGun.wire_selection)sb.Append('>');
                sb.Append(output.name);
            }
            
        }

        Label.Text = sb.ToString();
	}
}
*/