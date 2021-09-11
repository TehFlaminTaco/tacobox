public partial class SmartConstraintTool {
    public override string GetTitle(){
        return $"Smart {ConstraintTypeName}";
    }

    public override string GetDescription(){
        if(targetBody is null){
            return $"Left Click: Select First\nRight Click: Nudge In\nReload: Nudge Out";
        }else if(connectBody is null){
            return $"Left Click: Connect To\nReload: Reset";
        }else{
            return $"Left Click: Confirm\nReload: Reset";
        }
        //return ClassInfo.Description;
    }
}