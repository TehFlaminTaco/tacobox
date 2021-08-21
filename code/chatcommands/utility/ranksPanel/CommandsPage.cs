using System.Linq;
using Sandbox;
using Sandbox.UI;

public class Commands : RankPanel.Page {
    public override string Name => "Commands";

    CommandList Inherited;
    CommandList Allowed;
    CommandList Disallowed;

    public Commands(RankPanel.RankSettings parent) : base(parent) {
        Form form = new Form();
        AddChild(form);

        form.AddRow("Denied", Disallowed = new());
        form.AddRow("Use Inherited", Inherited = new());
        form.AddRow("Allowed", Allowed = new());
        UpdateChildren();
    }

    public void UpdateChildren(){
        Inherited.DeleteChildren( true );
        Allowed.DeleteChildren( true );
        Disallowed.DeleteChildren( true );
        var rank = Rank.FromName(parent.parent.currentRank);
        if (rank is null)return;
        foreach(var command in Command.commands.Values){
            var perm = rank.Commands.FirstOrDefault(x=>x.flag_or_command.ToLower() == command.Name.ToLower());
            if(perm is not null){
                if(perm.access == Rank.Permission.Access.Allow)
                    Allowed.AddChild(new CommandButtonAllow(this, command));
                if(perm.access == Rank.Permission.Access.Deny)
                    Disallowed.AddChild(new CommandButtonDisallow(this, command));
            }else{
                Inherited.AddChild(new CommandButtonInherit(this, command));
            }
        }
    }

    class CommandList : Panel {
        public CommandList(){
            AddClass("commandList");
        }
    }

    abstract class CommandButton : Panel {
        public Button push1;
        public Button push2;
        public Label name;
        Commands page;

        public CommandButton(Commands page, Command cmd){
            this.page = page;
            AddClass("commandButton");
            push1 = new();
            push2 = new();
            name = new Label{Text = cmd.Name};
        }
    }

    class CommandButtonInherit : CommandButton {
        public CommandButtonInherit(Commands page, Command cmd) : base(page, cmd){
            push1.Text = "❌";
            push2.Text = "✔️";

            AddChild(push1);
            AddChild(name);
            AddChild(push2);
            SetClass("allowed", Rank.FromName(page.parent.parent.currentRank).HasFlag("allCommands")||(Rank.FromName(page.parent.parent.currentRank).GetParent()?.HasCommand(cmd.Name)??false));

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasCommand(page.parent.parent.currentRank, cmd.Name, -1);
                page.Disallowed.AddChild(new CommandButtonDisallow(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasCommand(page.parent.parent.currentRank, cmd.Name, 1);
                page.Allowed.AddChild(new CommandButtonAllow(page, cmd));
            });
        }
    }

    class CommandButtonDisallow : CommandButton {
        public CommandButtonDisallow(Commands page, Command cmd) : base(page, cmd){
            push1.Text = "👪";
            push2.Text = "✔️";

            AddChild(name);
            AddChild(push1);
            AddChild(push2);

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasCommand(page.parent.parent.currentRank, cmd.Name, 0);
                page.Inherited.AddChild(new CommandButtonInherit(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasCommand(page.parent.parent.currentRank, cmd.Name, 1);
                page.Allowed.AddChild(new CommandButtonAllow(page, cmd));
            });
        }
    }

    class CommandButtonAllow : CommandButton {
        public CommandButtonAllow(Commands page, Command cmd) : base(page, cmd){
            push1.Text = "❌";
            push2.Text = "👪";

            AddChild(push1);
            AddChild(push2);
            AddChild(name);
            AddClass("allowed");

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasCommand(page.parent.parent.currentRank, cmd.Name, -1);
                page.Disallowed.AddChild(new CommandButtonDisallow(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasCommand(page.parent.parent.currentRank, cmd.Name, 0);
                page.Inherited.AddChild(new CommandButtonInherit(page, cmd));
            });
        }
    }
}