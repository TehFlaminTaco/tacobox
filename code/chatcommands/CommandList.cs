public partial class Command {
    public static void SetupCommands(){
        new CommandKill().Register();
        new CommandSlap().Register();
        new CommandSlay().Register();
        new CommandHP().Register();

        new CommandRanks().Register();
        new CommandSetRank().Register();
    }
}