using Sandbox;

public interface IGlowing{
    public bool ShouldGlow => true;

    static ModelEntity lastHovered=null;
    [Event.Tick]
    public static void GlowHovered(){
        if(Local.Pawn is null)return;
        var startPos = Local.Pawn.EyePos;
		var dir = Local.Pawn.EyeRot.Forward;
        if(lastHovered is not null && lastHovered.IsValid())lastHovered.GlowActive = false;

		var tr = Trace.Ray( startPos, startPos + dir * 300f )
			.Ignore( Local.Pawn )
			.Run();
		if(((tr.Entity is IGlowing ge && ge.ShouldGlow) || (tr.Entity is IWireEntity && Local.Pawn.ActiveChild is WireGun)) && tr.Entity is ModelEntity ent ){
			ent.GlowState = GlowStates.GlowStateLookAt;
			ent.GlowDistanceStart = 0;
			ent.GlowDistanceEnd = 1000;
			ent.GlowColor = Color.White;
			ent.GlowActive = true;
            lastHovered = ent;
		}
    }
}