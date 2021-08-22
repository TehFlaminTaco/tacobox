using System.Collections.Generic;
using Sandbox;

public partial class CommandRanks : Command {
    public override string Name => "Ranks";
    public override string Category => "Utility";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        ShowRankPanel(To.Single(executor));
        return true;
    }

    static RankPanel panel;
    [ClientRpc]
    public static void ShowRankPanel(){
        if(panel is not null)
            panel.Delete(true);
        panel = new RankPanel();
        SandboxHud.Instance.RootPanel.AddChild(panel);
    }
}