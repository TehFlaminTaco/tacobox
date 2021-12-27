using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public class PropsAllowed : RankPanel.Page
{
    List<PropIcon> allIcons = new();
	public PropsAllowed( RankPanel.RankSettings parent ) : base( parent ){
        foreach ( var file in FileSystem.Mounted.FindFile( "models", "*.vmdl_c.png", true ) )
		{
			if ( string.IsNullOrWhiteSpace( file ) ) continue;
			if ( file.Contains( "_lod0" ) ) continue;
			if ( file.Contains( "clothes" ) ) continue;

            var icon = new PropIcon(this, file.Remove( file.Length - 6 ));
            allIcons.Add(icon);
			this.AddChild(icon);
		}
        UpdateChildren();
    }

	public override string Name => "Props Allowed";

	public void UpdateChildren(){
        foreach(var icon in allIcons){
            icon.UpdateButtons();
        }
    }
    
    public class PropIcon : Panel {
        public PropsAllowed page;
        public Button denied = null;
        public Button inherited = null;
        public Button allowed = null;
        public Panel icon = null;
        string path;

        public PropIcon(PropsAllowed page, string path){
            this.path = path;
            this.page = page;
			icon = Add.Panel( "icon" );
			icon.Style.BackgroundImage = Texture.Load( $"/models/{path}_c.png", false );

            denied = new("❌", "", ()=>{
                setHas(path, -1);
                page.UpdateChildren();
            });
            inherited = new("👪", "", ()=>{
                setHas(path, 0);
                page.UpdateChildren();
            });
            allowed = new("✔️", "", ()=>{
                setHas(path, 1);
                page.UpdateChildren();
            });
            var allowState = Add.Panel("allowState");
            allowState.AddChild(denied);
            allowState.AddChild(inherited);
            allowState.AddChild(allowed);
        }

        public void setHas(string prop, int state){
            Rank.SetRankHasProp(page.parent.parent.currentRank, prop, state);
            var rank = Rank.FromName(page.parent.parent.currentRank);
            rank.AllowedProps.RemoveAll(c=>c.name.ToLower()==path.ToLower());
            if(state != 0){
                rank.AllowedProps.Add(new Rank.Permission{
                    name = path,
                    access = state == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
                });
            }
        }

        public void UpdateButtons(){
            var rank = Rank.FromName(page.parent.parent.currentRank);
            if(rank is null)return;
            int state = rank.CanSpawnProp(path)?1:-1;
            icon.SetClass("allowed", state==1);
            if(!rank.AllowedProps.Any(c=>c.name.ToLower()==path.ToLower()))
                state = 0;
            denied.SetClass("selected", state==-1);
            inherited.SetClass("selected", state==0);
            allowed.SetClass("selected", state==1);
        }
    }
}