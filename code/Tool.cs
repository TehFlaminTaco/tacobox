using Sandbox;
using Sandbox.Tools;
using Sandbox.UI;

[Library( "weapon_tool", Title = "Toolgun" )]
partial class Tool : Carriable
{
	[ConVar.ClientData( "tool_current" )]
	public static string UserToolCurrent { get; set; } = "tool_boxgun";

	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override int HoldSlot => 0;

	[Net, Predicted]
	public BaseTool CurrentTool { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override void Simulate( Client owner )
	{
		UpdateCurrentTool( owner );

		CurrentTool?.Simulate();
	}

	private void UpdateCurrentTool( Client owner )
	{
		var toolName = owner.GetClientData( "tool_current", "tool_boxgun" );
		if ( (toolName??"none") == "none" )
			return;
		
		if(!owner.HasTool(toolName)){
			if(IsClient){
				TacoChatBox.AddChatEntry("red", "", $"You don't have permission to use {toolName}", "debug/particleerror.vtex");
				ConsoleSystem.Run("tool_current none");
			}

			return;
		}

		// Already the right tool
		if ( CurrentTool != null && CurrentTool.Parent == this && CurrentTool.Owner == owner.Pawn && CurrentTool.ClassInfo.IsNamed( toolName ) )
			return;

		if ( CurrentTool != null )
		{
			CurrentTool?.Deactivate();
			CurrentTool = null;
		}

		CurrentTool = Library.Create<BaseTool>( toolName, false );

		if ( CurrentTool != null )
		{
			CurrentTool.Parent = this;
			CurrentTool.Owner = owner.Pawn as Player;
			CurrentTool.Activate();
			if(IsClient)
				CurrentTool.GenerateControls(SpawnMenu.Instance.inspector);
		}
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		CurrentTool?.Activate();
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		CurrentTool?.Deactivate();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		CurrentTool?.Deactivate();
		CurrentTool = null;
	}

	public override void OnCarryDrop( Entity dropper )
	{
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( Owner is Player player && player.ActiveChild != this )
			return;

		CurrentTool?.OnFrame();
	}
}

namespace Sandbox.Tools
{
	public partial class BaseTool : BaseNetworkable
	{
		public Tool Parent { get; set; }
		public Player Owner { get; set; }

		protected virtual float MaxTraceDistance => 10000.0f;

		public virtual void Activate()
		{
			CreatePreviews();
		}

		public virtual void Deactivate()
		{
			DeletePreviews();
		}

		public virtual void Simulate()
		{

		}

		public virtual void OnFrame()
		{
			UpdatePreviews();
		}

		public virtual void CreateHitEffects( Vector3 pos )
		{
			Parent?.CreateHitEffects( pos );
		}

		public virtual void ClearControls(Panel inspector){}
		public virtual void GenerateControls(Form inspector){}

		public virtual string GetTitle()
		{
			return ClassInfo.Title;
		}

		public virtual string GetDescription()
		{
			return ClassInfo.Description;
		}

		public virtual bool EyeLock(){
			return false;
		}
	}
}
