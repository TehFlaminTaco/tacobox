using System.Text;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

class WireHUD : HudEntity<RootPanel> {
    public WireHUD(WireGun gun){
        if(!IsClient)
            return;
        
        RootPanel.StyleSheet.Load("/ui/WireHud.scss");
        RootPanel.AddChild<WirePanel>().gun = gun;
    }
}

class WirePanel : Panel {
    public Label Label;
    public WireGun gun;

	public WirePanel()
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

        /*int i = 0;
        if(gun.selectedWV is not null && gun.wireTarget is IWireOutput wireOutput){
            StringBuilder sb = new StringBuilder();
            sb.Append("Outputs:");
            foreach(var output in wireOutput.GetOutputs()){
                sb.Append('\n');
                sb.Append(output.TypeName);
                sb.Append('\t');
                if(i++ == WireGun.wire_selection)sb.Append('>');
                sb.Append(output.name);
            }
            Label.SetContent(sb.ToString());
        }
        if(gun.selectedWV is null && gun.wireTarget is IWireInput wireInput){
            StringBuilder sb = new StringBuilder();
            sb.Append("Inputs:");
            foreach(var input in wireInput.GetInputs()){
                sb.Append('\n');
                sb.Append(input.TypeName);
                sb.Append('\t');
                if(i++ == WireGun.wire_selection)sb.Append('>');
                sb.Append(input.name);
                if(input.connectedOutput is not null){
                    var connected = (input.connectedOutput as IWireOutput).GetOutputs()[input.connectedIndex];
                    sb.Append(" -> ");
                    sb.Append(input.connectedOutput);
                    sb.Append(" [");
                    sb.Append(connected.name);
                    sb.Append(']');
                    
                }
            }
            Label.SetContent(sb.ToString());
        }*/
	}
}