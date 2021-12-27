﻿using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;

[Library]
public partial class SpawnList : Panel
{
	VirtualScrollPanel Canvas;
	List<(Panel panel, string path)> icons = new();

	public SpawnList()
	{
		AddClass( "spawnpage" );
		AddChild( out Canvas, "canvas" );

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemSize = new Vector2( 100, 100 );
		Canvas.OnCreateCell = ( cell, data ) =>
		{
			var file = (string)data;
			icons.Add((cell, file));
			var panel = cell.Add.Panel( "icon" );
			panel.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn", "models/" + file ) );
			panel.Style.BackgroundImage = Texture.Load( $"/models/{file}_c.png", false );
		};

		foreach ( var file in FileSystem.Mounted.FindFile( "models", "*.vmdl_c.png", true ) )
		{
			if ( string.IsNullOrWhiteSpace( file ) ) continue;
			if ( file.Contains( "_lod0" ) ) continue;
			if ( file.Contains( "clothes" ) ) continue;

			Canvas.AddItem( file.Remove( file.Length - 6 ) );
		}
	}

	public void UpdateVisible(){
		var rank = Local.Client.GetRank();
		if(rank is null)return;
		foreach((var cell, var prop) in icons){
			cell.SetClass("hidden", !rank.CanSpawnProp(prop));
		}
	}
}
