using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System.Linq;

[Library]
public partial class EntityList : Panel
{
	VirtualScrollPanel Canvas;

	public EntityList()
	{
		/*AddClass( "spawnpage" );
		AddChild( out Canvas, "canvas" );

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemSize = new Vector2( 100, 100 );
		Canvas.OnCreateCell = ( cell, data ) =>
		{
			var entry = (LibraryAttribute)data;
			var btn = cell.Add.Button( entry.Title );
			btn.AddClass( "icon" );
			btn.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn_entity", entry.Name ) );
			btn.Style.Background = new PanelBackground
			{
				Texture = Texture.Load( $"/entity/{entry.Name}.png", false )
			};
		};

		var ents = Library.GetAllAttributes<Entity>().Where( x => x.Spawnable ).OrderBy( x => x.Title ).ToArray();

		foreach ( var entry in ents )
		{
			Canvas.AddItem( entry );
		}*/
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
			}
		}
	}
}
