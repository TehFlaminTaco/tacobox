using System.Linq;
using Sandbox;
using Sandbox.UI;

public class Flags : RankPanel.Page {
    public override string Name => "Flags";

    FlagList Inherited;
    FlagList Allowed;
    FlagList Disallowed;

    public Flags(RankPanel.RankSettings parent) : base(parent) {
        Form form = new Form();
        AddChild(form);

        form.AddRow("Removed Flag", Disallowed = new());
        form.AddRow("Use Inherited", Inherited = new());
        form.AddRow("Has Flag", Allowed = new());

        UpdateChildren();
    }

    public void UpdateChildren(){
        Inherited.DeleteChildren( true );
        Allowed.DeleteChildren( true );
        Disallowed.DeleteChildren( true );
        var rank = Rank.FromName(parent.parent.currentRank);
        if (rank is null)return;
        foreach(var command in AdminCore.AllFlags()){
            var perm = rank.Flags.FirstOrDefault(x=>x.flag_or_command.ToLower() == command.ToLower());
            if(perm is not null){
                if(perm.access == Rank.Permission.Access.Allow)
                    Allowed.AddChild(new FlagButtonAllow(this, command));
                if(perm.access == Rank.Permission.Access.Deny)
                    Disallowed.AddChild(new FlagButtonDisallow(this, command));
            }else{
                Inherited.AddChild(new FlagButtonInherit(this, command));
            }
        }
    }

    class FlagList : Panel {
        public FlagList(){
            AddClass("flagList");
        }
    }

    abstract class FlagButton : Panel {
        public Button push1;
        public Button push2;
        public Label name;
        Flags page;

        public FlagButton(Flags page, string cmd){
            this.page = page;
            AddClass("flagButton");
            push1 = new();
            push2 = new();
            name = new Label{Text = cmd};
        }
    }

    class FlagButtonInherit : FlagButton {
        public FlagButtonInherit(Flags page, string cmd) : base(page, cmd){
            push1.Text = "❌";
            push2.Text = "✔️";

            AddChild(push1);
            AddChild(name);
            AddChild(push2);
            SetClass("allowed", Rank.FromName(page.parent.parent.currentRank).GetParent()?.HasFlag(cmd)??false);

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasFlag(page.parent.parent.currentRank, cmd, -1);
                page.Disallowed.AddChild(new FlagButtonDisallow(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasFlag(page.parent.parent.currentRank, cmd, 1);
                page.Allowed.AddChild(new FlagButtonAllow(page, cmd));
            });
        }
    }

    class FlagButtonDisallow : FlagButton {
        public FlagButtonDisallow(Flags page, string cmd) : base(page, cmd){
            push1.Text = "👪";
            push2.Text = "✔️";

            AddChild(name);
            AddChild(push1);
            AddChild(push2);

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasFlag(page.parent.parent.currentRank, cmd, 0);
                page.Inherited.AddChild(new FlagButtonInherit(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasFlag(page.parent.parent.currentRank, cmd, 1);
                page.Allowed.AddChild(new FlagButtonAllow(page, cmd));
            });
        }
    }

    class FlagButtonAllow : FlagButton {
        public FlagButtonAllow(Flags page, string cmd) : base(page, cmd){
            push1.Text = "❌";
            push2.Text = "👪";

            AddChild(push1);
            AddChild(push2);
            AddChild(name);
            AddClass("allowed");

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasFlag(page.parent.parent.currentRank, cmd, -1);
                page.Disallowed.AddChild(new FlagButtonDisallow(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasFlag(page.parent.parent.currentRank, cmd, 0);
                page.Inherited.AddChild(new FlagButtonInherit(page, cmd));
            });
        }
    }
}