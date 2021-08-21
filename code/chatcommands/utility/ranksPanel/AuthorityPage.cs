using System.Linq;
using Sandbox;
using Sandbox.UI;

public class Authoritys : RankPanel.Page {
    public override string Name => "Authorities";

    AuthorityList Inherited;
    AuthorityList Allowed;
    AuthorityList Disallowed;

    public Authoritys(RankPanel.RankSettings parent) : base(parent) {
        Form form = new Form();
        AddChild(form);

        form.AddRow("Cannot Affect", Disallowed = new());
        form.AddRow("Use Inherited", Inherited = new());
        form.AddRow("Can Affect", Allowed = new());

        UpdateChildren();
    }

    public void UpdateChildren(){
        Inherited.DeleteChildren( true );
        Allowed.DeleteChildren( true );
        Disallowed.DeleteChildren( true );
        var rank = Rank.FromName(parent.parent.currentRank);
        if (rank is null)return;
        foreach(var r in AdminCore.ranks.OrderBy(c=>-c.Authority)){
            var perm = rank.AuthorityOver.FirstOrDefault(x=>x.flag_or_command.ToLower() == r.Name.ToLower());
            if(perm is not null){
                if(perm.access == Rank.Permission.Access.Allow)
                    Allowed.AddChild(new AuthorityButtonAllow(this, r.Name));
                if(perm.access == Rank.Permission.Access.Deny)
                    Disallowed.AddChild(new AuthorityButtonDisallow(this, r.Name));
            }else{
                Inherited.AddChild(new AuthorityButtonInherit(this, r.Name));
            }
        }
    }
    class AuthorityList : Panel {
        public AuthorityList(){
            AddClass("authorityList");
        }
    }

    abstract class AuthorityButton : Panel {
        public Button push1;
        public Button push2;
        public Label name;
        Authoritys page;

        public AuthorityButton(Authoritys page, string cmd){
            this.page = page;
            AddClass("authorityButton");
            push1 = new();
            push2 = new();
            name = new Label{Text = cmd};
        }
    }

    class AuthorityButtonInherit : AuthorityButton {
        public AuthorityButtonInherit(Authoritys page, string cmd) : base(page, cmd){
            push1.Text = "❌";
            push2.Text = "✔️";

            AddChild(push1);
            AddChild(name);
            AddChild(push2);
            SetClass("allowed", Rank.FromName(page.parent.parent.currentRank).HasFlag("allAuthority")||(Rank.FromName(page.parent.parent.currentRank).GetParent()?.CanTouch(cmd)??false));

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasAuthority(page.parent.parent.currentRank, cmd, -1);
                page.Disallowed.AddChild(new AuthorityButtonDisallow(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasAuthority(page.parent.parent.currentRank, cmd, 1);
                page.Allowed.AddChild(new AuthorityButtonAllow(page, cmd));
            });
        }
    }

    class AuthorityButtonDisallow : AuthorityButton {
        public AuthorityButtonDisallow(Authoritys page, string cmd) : base(page, cmd){
            push1.Text = "👪";
            push2.Text = "✔️";

            AddChild(name);
            AddChild(push1);
            AddChild(push2);

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasAuthority(page.parent.parent.currentRank, cmd, 0);
                page.Inherited.AddChild(new AuthorityButtonInherit(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasAuthority(page.parent.parent.currentRank, cmd, 1);
                page.Allowed.AddChild(new AuthorityButtonAllow(page, cmd));
            });
        }
    }

    class AuthorityButtonAllow : AuthorityButton {
        public AuthorityButtonAllow(Authoritys page, string cmd) : base(page, cmd){
            push1.Text = "❌";
            push2.Text = "👪";

            AddChild(push1);
            AddChild(push2);
            AddChild(name);
            AddClass("allowed");

            push1.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasAuthority(page.parent.parent.currentRank, cmd, -1);
                page.Disallowed.AddChild(new AuthorityButtonDisallow(page, cmd));
            });
            push2.AddEventListener("onclick", e=> {
                Delete(false);
                Rank.SetRankHasAuthority(page.parent.parent.currentRank, cmd, 0);
                page.Inherited.AddChild(new AuthorityButtonInherit(page, cmd));
            });
        }
    }
}