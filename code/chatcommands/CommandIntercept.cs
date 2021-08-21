
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

        if(message[0] == '/' || message[0] == '!' || message[0] == '@'){
            // TODO: better split arguments
            var cmdargs = argMatch.Matches(message.Substring(1)).Select(c=>c.Groups["body"].Value).ToArray();
            if(Command.commands.ContainsKey(cmdargs[0].ToLower())){
                var cmd = Command.commands[cmdargs[0].ToLower()];
                if(ConsoleSystem.Caller.HasCommand(cmd.Name)){
                    if(message[0]!='@'){
                        TacoChatBox.AddChatEntry( To.Everyone, ConsoleSystem.Caller.GetRank().NameColor, $"[{ConsoleSystem.Caller.GetRank().Name}] {ConsoleSystem.Caller.Name} ran {cmd.Name}", "", $"avatar:{ConsoleSystem.Caller.SteamId}" );
                    }else{
                        TacoChatBox.AddChatEntry( AdminCore.SeeSilent(ConsoleSystem.Caller, true), ConsoleSystem.Caller.GetRank().NameColor, $"[{ConsoleSystem.Caller.GetRank().Name}] {ConsoleSystem.Caller.Name} ran {cmd.Name} silently", "", $"avatar:{ConsoleSystem.Caller.SteamId}" );
                    }
                    cmd.Run(ConsoleSystem.Caller.Pawn as Player, cmdargs.Skip(1), message[0]=='@');
                }else{
                    TacoChatBox.AddChatEntry( AdminCore.SeeSilent(null as Client, true), ConsoleSystem.Caller.GetRank().NameColor, $"[{ConsoleSystem.Caller.GetRank().Name}] {ConsoleSystem.Caller.Name} ran {cmd.Name}, but lacked access.", "", $"avatar:{ConsoleSystem.Caller.SteamId}" );
                    TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "You don't have permission to run that!", "debug/particleerror.vtex");
                }
            }else{
                TacoChatBox.AddChatEntry(To.Single(ConsoleSystem.Caller), "red", "", "Command not found!", "debug/particleerror.vtex");
            }
        }else{
            TacoChatBox.AddChatEntry( To.Everyone, ConsoleSystem.Caller.GetRank().NameColor, $"[{ConsoleSystem.Caller.GetRank().Name}] {ConsoleSystem.Caller.Name}", message, $"avatar:{ConsoleSystem.Caller.SteamId}" );
        }

        //message = message.Replace("a", "B");
    }
}