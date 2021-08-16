
using Sandbox;
using Sandbox.UI;

public class CommandIntercept {
    [ServerCmd( "say" )]
    public static void Say( string message )
    {
        Assert.NotNull( ConsoleSystem.Caller );

        // todo - reject more stuff
        if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
            return;

        //message = message.Replace("a", "B");

        Log.Info( $"{ConsoleSystem.Caller}: {message}" );
        ChatBox.AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}" );
    }
}