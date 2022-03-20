using System.Runtime.CompilerServices;
using System.Threading;

using Sandbox;
using Sandbox.Tools;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

[Library]
public partial class SpawnMenu : Panel
{
	public static SpawnMenu Instance;
	readonly Panel toollist;
	public readonly Form inspector;
	readonly EntityList ents;
	readonly SpawnList spawnList;
	readonly CloudModelList cloudList;

	public SpawnMenu()
	{
		Instance = this;

		StyleSheet.Load( "/ui/SpawnMenu.scss" );

		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );

			var body = left.Add.Panel( "body" );

			{
				spawnList = body.AddChild<SpawnList>();
				tabs.SelectedButton = tabs.AddButtonActive( "Props", ( b ) => spawnList.SetClass( "active", b ) );

				ents = body.AddChild<EntityList>();
				tabs.AddButtonActive( "Entities", ( b ) => ents.SetClass( "active", b ) );

				cloudList = body.AddChild<CloudModelList>();
				tabs.AddButtonActive( "s&works", ( b ) => cloudList.SetClass( "active", b));
			}
		}

		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
				tabs.Add.Button( "Tools" ).AddClass( "active" );
				tabs.Add.Button( "Utility" );
			}
			var body = right.Add.Panel( "body" );
			{
				toollist = body.Add.Panel( "toollist" );
				var insp = body.Add.Panel( "inspector" );
				insp.StyleSheet.Load( "/styles/UITests.scss" );
				insp.AddChild(inspector=new Form());
				RebuildToolList();
			}
		}

	}

	BaseTool GetCurrentTool()
	{
		if(Local.Pawn is not SandboxPlayer player)return null;

		var inventory = player.Inventory;
		if ( inventory == null ) return null;

		if ( inventory.Active is not Tool tool ) return null;

		return tool?.CurrentTool;
	}

	List<(Button button, string tool)> toolButtons = new();
	public void UpdateToolsVisible(){
		try{
			spawnList.UpdateVisible();
			ents.UpdateVisible();
		}catch(Exception){}
		try{
			var hideInvalidTools = !Local.Client.HasFlag("showDeniedTools");
			foreach((var button, var tool) in toolButtons){
				button.SetClass("denied", !Local.Client.HasTool(tool));
				button.SetClass("hidden", hideInvalidTools&&!Local.Client.HasTool(tool));
			}
		}catch(Exception){}
	}

	void RebuildToolList()
	{
		toollist.DeleteChildren( true );
		inspector.DeleteChildren( true );
		ents.Reload();
		toolButtons.Clear();

		foreach ( var kv in Library.GetAllAttributes<Sandbox.Tools.BaseTool>().GroupBy(x=>x.Group) )
		{
			
			toollist.Add.Label(kv.Key, "category");

			foreach(var entry in kv){
				if ( entry.Title == "BaseTool" )
				continue;

				var button = toollist.Add.Button( entry.Title );
				toolButtons.Add((button, entry.Name));
				button.SetClass( "active", entry.Name == ConsoleSystem.GetValue( "tool_current" ) );

				button.AddEventListener( "onclick", () =>
				{
					if(!button.HasClass("active")){
						GetCurrentTool()?.ClearControls(inspector);
						inspector.DeleteChildren(true);
					}
					ConsoleSystem.Run( "tool_current", entry.Name );
					ConsoleSystem.Run( "inventory_current", "weapon_tool" );
					
					foreach ( var child in toollist.Children )
						child.SetClass( "active", child == button );
					
					
				} );
			}
		}
		
		if(AdminCore.Setup) UpdateToolsVisible();
		GetCurrentTool()?.GenerateControls(inspector);
	}

	bool lockOpen = false;
	public override void Tick()
	{
		base.Tick();
		
		if(Input.Down(InputButton.Menu) && inspector.HasFocus && !lockOpen) lockOpen = true;
		if(Input.Down(InputButton.Menu) && !inspector.HasFocus && lockOpen) lockOpen = false;

		Parent.SetClass( "spawnmenuopen", Input.Down( InputButton.Menu ) || lockOpen );
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RebuildToolList();
	}
}
