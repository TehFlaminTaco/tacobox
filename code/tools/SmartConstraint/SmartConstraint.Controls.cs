using System;
using Sandbox;
using Sandbox.Tools;
using Sandbox.UI;

public partial class SmartConstraintTool {
    [ConVar.ClientData] public static string smartconstraint_dorotate {get; set;} = "1";
    public bool DoRotation => Local.Pawn is null ? Owner.GetClientOwner().GetClientData("smartconstraint_dorotate").ToBool() : smartconstraint_dorotate.ToBool();

    [ConVar.ClientData] public static string smartconstraint_snaprotation {get; set;} = "1";
    public bool SnapRotation => Local.Pawn is null ? Owner.GetClientOwner().GetClientData("smartconstraint_snaprotation").ToBool() : smartconstraint_snaprotation.ToBool();

    [ConVar.ClientData] public static string smartconstraint_snapdegrees {get; set;} = "30";
    public float SnapDegrees => Local.Pawn is null ? Owner.GetClientOwner().GetClientData("smartconstraint_snapdegrees").ToFloat() : smartconstraint_snapdegrees.ToFloat();

    [ConVar.ClientData] public static string smartconstraint_offset {get; set;} = "0";
    public float Offset => Local.Pawn is null ? Owner.GetClientOwner().GetClientData("smartconstraint_offset").ToFloat() : smartconstraint_offset.ToFloat();

    [ConVar.ClientData] public static string smartconstraint_nudgepercent {get; set;} = "1";
    public bool NudgePercent => Local.Pawn is null ? Owner.GetClientOwner().GetClientData("smartconstraint_nudgepercent").ToBool() : smartconstraint_nudgepercent.ToBool();

    [ConVar.ClientData] public static string smartconstraint_nudgeamount {get; set;} = "100";
    public float NudgeAmount => Local.Pawn is null ? Owner.GetClientOwner().GetClientData("smartconstraint_nudgeamount").ToFloat() : smartconstraint_nudgeamount.ToFloat();

    [ConVar.ClientData] public static string smartconstraint_constraint {get; set;} = "Weld";
    public string ConstraintTypeName => Local.Pawn is null ? Owner.GetClientOwner().GetClientData("smartconstraint_constraint") : smartconstraint_constraint;


    public override void GenerateControls(Form inspector){
        var checkDoRotation = new Checkbox{
            LabelText = "Do Rotation",
            Checked = DoRotation
        };
        inspector.AddRow("",checkDoRotation);

        var checkSnapRotation = new Checkbox{
            LabelText = "Snap Rotation",
            Checked = SnapRotation
        };
        checkSnapRotation.SetClass("disabled", !DoRotation);
        inspector.AddRow("",checkSnapRotation);

        var textSnapDegrees = new TextEntry{
            Numeric = true,
            Text = smartconstraint_snapdegrees
        };
        textSnapDegrees.SetClass("disabled", !SnapRotation || !DoRotation);
        inspector.AddRow("Snap Degrees",textSnapDegrees);

        var textOffset = new TextEntry{
            Numeric = true,
            Text = smartconstraint_offset
        };
        inspector.AddRow("Offset", textOffset);

        var checkNudgePercent = new Checkbox{
            LabelText = "Nudge As Percent",
            Checked = NudgePercent
        };
        inspector.AddRow("",checkNudgePercent);
        
        var textNudgeAmount = new TextEntry{
            Numeric = true,
            Text = smartconstraint_nudgeamount
        };
        inspector.AddRow("Nudge Amount", textNudgeAmount);

        var menuConstraintType = new DropDown();
        foreach(var typ in Enum.GetValues<ConstraintType>()){
            var opt = new Option{
                Title = typ.ToString(),
                Value = typ
            };
            menuConstraintType.Options.Add(opt);
            if(typ.ToString().ToLower() == ConstraintTypeName.ToLower()){
                menuConstraintType.Selected = opt;
            }
        }
        inspector.AddRow("Type", menuConstraintType);

        checkDoRotation.AddEventListener("onchange", e=>{
            smartconstraint_dorotate = ""+checkDoRotation.Checked;
            ConsoleSystem.Run("smartconstraint_dorotate "+checkDoRotation.Checked);
            checkSnapRotation.SetClass("disabled", !DoRotation);
            textSnapDegrees.SetClass("disabled", !SnapRotation || !DoRotation);
        });
        checkSnapRotation.AddEventListener("onchange", e=>{
            smartconstraint_snaprotation = ""+checkSnapRotation.Checked;
            ConsoleSystem.Run("smartconstraint_snaprotation "+checkSnapRotation.Checked);
            textSnapDegrees.SetClass("disabled", !SnapRotation || !DoRotation);
        });
        textSnapDegrees.AddEventListener("onchange", e=>{
            smartconstraint_snapdegrees = ""+textSnapDegrees.Text;
            ConsoleSystem.Run("smartconstraint_snapdegrees "+textSnapDegrees.Text);
        });
        textOffset.AddEventListener("onchange", e=>{
            smartconstraint_offset = ""+textOffset.Text;
            ConsoleSystem.Run("smartconstraint_offset "+textOffset.Text);
        });
        checkNudgePercent.AddEventListener("onchange",e=>{
            smartconstraint_nudgepercent = ""+checkNudgePercent.Checked;
            ConsoleSystem.Run("smartconstraint_nudgepercent "+checkNudgePercent.Checked);
        });
        textNudgeAmount.AddEventListener("onchange", e=>{
            smartconstraint_nudgeamount = ""+textNudgeAmount.Text;
            ConsoleSystem.Run("smartconstraint_nudgeamount "+textNudgeAmount.Text);
        });
        menuConstraintType.AddEventListener("value.changed", e=>{
            smartconstraint_constraint = ""+menuConstraintType.Selected.Title;
            ConsoleSystem.Run("smartconstraint_constraint "+menuConstraintType.Selected.Title);
        });
    }
}