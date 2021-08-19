using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

public class RenderTargets : Panel {
    public static List<Action> Render = new();

    public override void DrawBackground( ref RenderState state )
    {
        base.DrawBackground( ref state );
        if(Local.Pawn == null)return;

        foreach(var act in Render)try{act();}catch(Exception){};
        Render.Clear();
    }
}