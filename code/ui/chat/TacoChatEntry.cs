using Sandbox.UI.Construct;


namespace Sandbox.UI
{
	public partial class TacoChatEntry : Panel
	{
		public Label NameLabel { get; internal set; }
		public MarkdownLabel Message { get; internal set; }
		public Image Avatar { get; internal set; }

		public RealTimeSince TimeSinceBorn = 0;

		public TacoChatEntry()
		{
			Avatar = Add.Image();
			NameLabel = Add.Label( "Name", "name" );
			AddChild(Message = new MarkdownLabel( "Message", "message" ));
		}

		public override void Tick() 
		{
			base.Tick();

			if ( TimeSinceBorn > 10 ) 
			{ 
				Delete();
			}
		}
	}
}