
using Sandbox;
using Sandbox.UI;
using System.Linq;
using System.Text.RegularExpressions;

public class CommandIntercept {
    public static Regex argMatch = new Regex(@"""(?<body>(.|\\\""))*?""|(?<body>\S+)");

    [ServerCmd( "say" )]
    public static void Say( string message )
    {
        Assert.NotNull( ConsoleSystem.Caller );
        if(message.Length == 0)return;

        // todo - reject more stuff
        if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
            return;

        if(message[0] == '/'){
            // TODO: better split arguments
            var cmdargs = argMatch.Matches(message.Substring(1)).Select(c=>c.Groups["body"].Value).ToArray();
            if(Command.commands.ContainsKey(cmdargs[0].ToLower())){
                var cmd = Command.commands[cmdargs[0].ToLower()];
                if(ConsoleSystem.Caller.HasCommand(cmd.Name))
                    cmd.Run(ConsoleSystem.Caller.Pawn as Player, cmdargs.Skip(1));
                else
                    TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You don't have permission to run that!", "debug/particleerror.vtex");
            }
        }

        //message = message.Replace("a", "B");
        TacoChatBox.AddChatEntry( To.Everyone, ConsoleSystem.Caller.GetRank().NameColor, $"[{ConsoleSystem.Caller.GetRank().Name}] {ConsoleSystem.Caller.Name}", message, $"avatar:{ConsoleSystem.Caller.SteamId}" );
    }
}