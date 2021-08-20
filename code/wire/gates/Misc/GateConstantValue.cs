using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class GateConstantValue : Gate
{
    [ConVar.ClientData]
    public static string gate_constantvalue_type {get; set;} = "Normal";
    [ConVar.ClientData]
    public static string gate_constantvalue_body {get; set;} = "1.0";

    public GateConstantValue(){
        Key = "misc_constantvalue";
        Name = "Constant Value";
        Description = "A preset, constant value.";
        Inputs = Array.Empty<(WireVal.Type t, string key, string name)>();
        Outputs = new[]{
            (WireVal.Type.Normal, "Out", "Out")
        };
    }
	public override void Process( GateEntity ent )
	{
		return;
	}

    public override List<WireVal> GenerateValues(Player ply, GateEntity ent){
        List<WireVal> vals = new();
        var typ = ply.IsClient ? gate_constantvalue_type : ply.GetClientOwner().GetUserString("gate_constantvalue_type");
        WireVal.Type t = WireVal.Type.String;
        Enum.TryParse(typ, true, out t);
        var storeVal = WireVal.FromType(t, "Out", "Out", WireVal.Direction.Output);
        storeVal.val.ParseFromString(ply.IsClient ? gate_constantvalue_body : ply.GetClientOwner().GetUserString("gate_constantvalue_body"));
        vals.Add(storeVal.val);

        return vals;
    }

    public override void GenerateControls(Form inspector){
        var buttons = new Panel();
        List<(WireVal.Type type, Button button)> allbtns = new();

        //var types = new DropDown();
        foreach(var typof in Enum.GetValues<WireVal.Type>()){
            var button = buttons.Add.Button(typof.ToString());
            allbtns.Add((typof, button));
            button.Style.FontSize = Length.Pixels(10);
            button.Style.Dirty();
            button.SetClass("active", gate_constantvalue_type.ToLower()==typof.ToString().ToLower());
            button.AddEventListener("onclick", e=>{
                gate_constantvalue_type = typof.ToString();
                ConsoleSystem.Run("gate_constantvalue_type "+gate_constantvalue_type);
                foreach(var btn in allbtns)
                    btn.button.SetClass("active", btn.type == typof);
            });
        }
        inspector.AddRow("Type", buttons);
        var txtEntry = new TextEntry();
        txtEntry.Text = gate_constantvalue_body;
        txtEntry.AddEventListener("onchange", e=>{
            gate_constantvalue_body = txtEntry.Text;
            ConsoleSystem.Run("gate_constantvalue_body "+gate_constantvalue_body);
        });
        inspector.AddRow("Value", txtEntry);
    }
}