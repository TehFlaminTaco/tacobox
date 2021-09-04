using Sandbox;

public interface IGlowing{
    public bool ShouldGlow => true;

    static ModelEntity lastHovered=null;
    [Event.Tick]
    public static void GlowHovered(){
        if(Local.Pawn is not SandboxPlayer ply)return;
        if(lastHovered is not null && lastHovered.IsValid())lastHovered.GlowActive = false;

		var tr = ply.EyeTrace();
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