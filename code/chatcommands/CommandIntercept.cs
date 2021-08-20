
using Sandbox;
using Sandbox.UI;
using System.Linq;

public class CommandIntercept {
    [ServerCmd( "say" )]
    public static void Say( string message )
    {
        Assert.NotNull( ConsoleSystem.Caller );
        if(message.Length == 0)return;

        // todo - reject more stuff
        if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
            return;

        if(message[0] == '!'){
            // TODO: better split arguments
            var cmdargs = message.Substring(1).Split(' ');
            if(Command.commands.ContainsKey(cmdargs[0].ToLower())){
                Command.commands[cmdargs[0]].Run(ConsoleSystem.Caller.Pawn as Player, cmdargs.Skip(1));
            }
        }

        //message = message.Replace("a", "B");

        Log.Info( $"{ConsoleSystem.Caller}: {message}" );
        ChatBox.AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}" );
    }
}