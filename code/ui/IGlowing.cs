using Sandbox;
using Sandbox.Component;

public interface IGlowing{
    public bool ShouldGlow => true;

    static ModelEntity lastHovered=null;
    [Event.Tick]
    public static void GlowHovered(){
        if(Local.Pawn is not SandboxPlayer ply)return;
        if(lastHovered is not null && lastHovered.IsValid())
            if(lastHovered.Components.TryGet<Glow>(out var glw))
                glw.Active = false;

		var tr = ply.EyeTrace();
		if(((tr.Entity is IGlowing ge && ge.ShouldGlow) || (tr.Entity is IWireEntity && ply.ActiveChild is WireGun)) && tr.Entity is ModelEntity ent ){
            var glw = tr.Entity.Components.GetOrCreate<Glow>();
			glw.RangeMin = 0;
			glw.RangeMax = 1000;
			glw.Color = Color.White;
			glw.Active = true;
            lastHovered = ent;
		}
    }
}