using System.Runtime.CompilerServices;
using System.Threading;

using Sandbox;
using Sandbox.Tools;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

[Library]
public partial class SpawnMenu : Panel
{
	public static SpawnMenu Instance;
	readonly Panel toollist;
	public readonly Panel inspector;
	readonly EntityList ents;

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
				var props = body.AddChild<SpawnList>();
				tabs.SelectedButton = tabs.AddButtonActive( "Props", ( b ) => props.SetClass( "active", b ) );

				ents = body.AddChild<EntityList>();
				tabs.AddButtonActive( "Entities", ( b ) => ents.SetClass( "active", b ) );
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
				inspector = body.Add.Panel( "inspector" );
				{
					RebuildToolList();
				}
			}
		}

	}

	BaseTool GetCurrentTool()
	{
		var player = Local.Pawn;
		if ( player == null ) return null;

		var inventory = player.Inventory;
		if ( inventory == null ) return null;

		if ( inventory.Active is not Tool tool ) return null;

		return tool?.CurrentTool;
	}

	void RebuildToolList()
	{
		toollist.DeleteChildren( true );
		inspector.DeleteChildren( true );
		ents.Reload();

		foreach ( var entry in Library.GetAllAttributes<Sandbox.Tools.BaseTool>() )
		{
			if ( entry.Title == "BaseTool" )
				continue;

			var button = toollist.Add.Button( entry.Title );
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
