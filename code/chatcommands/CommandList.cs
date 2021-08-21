public partial class Command {
    public static void SetupCommands(){
        new CommandKill().Register();

        new CommandRanks().Register();
        new CommandSetRank().Register();
    }
}