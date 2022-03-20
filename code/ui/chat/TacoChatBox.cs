
using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Sandbox.UI
{
	public partial class TacoChatBox : Panel
	{
		static TacoChatBox Current;

		public Panel Canvas { get; protected set; }
		public TextEntry Input { get; protected set; }

		public TacoChatBox()
		{
			Current = this;

			StyleSheet.Load( "/ui/chat/TacoChatBox.scss" );

			Canvas = Add.Panel( "chat_canvas" );

			Input = Add.TextEntry( "" );
			Input.AddEventListener( "onsubmit", () => Submit() );
			Input.AddEventListener( "onblur", () => Close() );
			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;

			Sandbox.Hooks.TacoChat.OnOpenChat += Open;
		}


		void Open()
		{
			AddClass( "open" );
			Input.Focus();
		}

		void Close()
		{
			RemoveClass( "open" );
			Input.Blur();
		}

		void Submit()
		{
			Close();

			var msg = Input.Text.Trim();
			Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			CommandIntercept.Say( msg );
		}

		public void AddEntry( Color nameColor, string name, string message, string avatar )
		{
			var e = Canvas.AddChild<TacoChatEntry>();
			//e.SetFirstSibling();
			e.Message.Text = message;
			e.NameLabel.Text = name;
			e.NameLabel.Style.FontColor = nameColor;
			e.NameLabel.Style.Dirty();
			e.Avatar.SetTexture( avatar );

			e.SetClass( "noname", string.IsNullOrEmpty( name ) );
			e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );
		}


		[ClientCmd( "chat_add", CanBeCalledFromServer = true )]
		public static void AddChatEntry( string nameColor, string name, string message, string avatar = null )
		{
			Current?.AddEntry( Color.Parse(nameColor)??Color.White, name, message, avatar );

			// Only log clientside if we're not the listen server host
			//if ( !Global.IsListenServer )
			//{
				Log.Info( $"CHAT:: {name}: {message}" ); 
			//}
		}

		[ClientCmd( "chat_addinfo", CanBeCalledFromServer = true )]
		public static void AddInformation( string message, string avatar = null )
		{
			Current?.AddEntry( Color.White, null, message, avatar );
		}

	}
}

namespace Sandbox.Hooks
{
	public static partial class TacoChat
	{
		public static event Action OnOpenChat;

		[ClientCmd( "openchat" )]
		internal static void MessageMode()
		{
			OnOpenChat?.Invoke();
		}

	}
}
