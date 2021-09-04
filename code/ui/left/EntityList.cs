using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System.Collections.Generic;
using System.Linq;

[Library]
public partial class EntityList : Panel
{
	List<(Panel panel, string path)> icons = new();

	public EntityList()
	{
		Reload();
	}

	public void Reload(){
		DeleteChildren(true);
		var scrollBox = Add.Panel("scrollBox");

		var ents = Library.GetAllAttributes<Entity>().Where( x => x.Spawnable ).OrderBy( x => x.Title ).GroupBy(x=>x.Group).OrderBy( x => x.Key );

		foreach(var e in ents){
			scrollBox.Add.Label(e.Key??"Misc", "category");
			var entsBox = scrollBox.Add.Panel("box");
			foreach(var ent in e){
				var entry = entsBox.Add.Button("", "entry");
				var icon = entry.Add.Panel("icon");
				icon.Style.Background = new PanelBackground
				{
					Texture = Texture.Load( $"/entity/{ent.Name}.png", false )
				};
				entry.Add.Label(ent.Title);
				
				entry.AddEventListener("onclick", E=>{
					ConsoleSystem.Run( "spawn_entity", ent.Name );
				});
				icons.Add((entry, ent.Name));
			}
		}
	}

	public void UpdateVisible(){
		var rank = Local.Client.GetRank();
		if(rank is null)return;
		foreach((var cell, var prop) in icons){
			cell.SetClass("hidden", !rank.CanSpawnEnt(prop));
		}
	}
}
