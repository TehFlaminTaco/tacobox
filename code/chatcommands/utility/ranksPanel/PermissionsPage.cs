using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Permissions : RankPanel.Page {
    public override string Name => "Permissions";

    PermissionList Commands;
    PermissionList Flags;
    PermissionList Authorities;

    public Permissions(RankPanel.RankSettings parent) : base(parent) {
        Form form = new Form();
        AddChild(form);

        form.AddRow("Commands", Commands = new());
        form.AddRow("Flags", Flags = new());
        form.AddRow("Authorities", Authorities = new());

        UpdateChildren();
    }

    public void UpdateAllButtons(){
        foreach(var btn in Commands.ChildrenOfType<PermissionButton>()
                            .Union(Flags.ChildrenOfType<PermissionButton>())
                            .Union(Authorities.ChildrenOfType<PermissionButton>())){
            btn.SetClass("allowed", btn.getHas(btn.cmd));
            btn.UpdateButtons();
        }
        
    }

    public void UpdateChildren(){
        Commands.DeleteChildren( true );
        Flags.DeleteChildren( true );
        Authorities.DeleteChildren( true );

        Func<string, bool> hasCommand = s=>Rank.FromName(parent.parent.currentRank).HasCommand(s.ToLower());
        Func<string, bool> commandInherited = s=>!Rank.FromName(parent.parent.currentRank).Commands.Any(c=>c.flag_or_command.ToLower()==s.ToLower());
        Action<string, int> setCommand = (s,i)=>{
            var rank = Rank.FromName(parent.parent.currentRank);
            rank.Commands.RemoveAll(c=>c.flag_or_command.ToLower() == s.ToLower());
            if(i != 0)
                rank.Commands.Add(new Rank.Permission{
                    flag_or_command = s,
                    access = i == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
                });
            Rank.SetRankHasCommand(parent.parent.currentRank, s, i);
        };

        Func<string, bool> hasFlag = s=>Rank.FromName(parent.parent.currentRank).HasFlag(s.ToLower());
        Func<string, bool> flagInherited = s=>!Rank.FromName(parent.parent.currentRank).Flags.Any(c=>c.flag_or_command.ToLower()==s.ToLower());
        Action<string, int> setFlag = (s,i)=>{
            var rank = Rank.FromName(parent.parent.currentRank);
            rank.Flags.RemoveAll(c=>c.flag_or_command.ToLower() == s.ToLower());
            if(i != 0)
                rank.Flags.Add(new Rank.Permission{
                    flag_or_command = s,
                    access = i == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
                });
            Rank.SetRankHasFlag(parent.parent.currentRank, s, i);
        };

        Func<string, bool> hasAuthority = s=>Rank.FromName(parent.parent.currentRank).CanTouch(s.ToLower());
        Func<string, bool> authorityInherited = s=>!Rank.FromName(parent.parent.currentRank).AuthorityOver.Any(c=>c.flag_or_command.ToLower()==s.ToLower());
        Action<string, int> setAuthority = (s,i)=>{
            var rank = Rank.FromName(parent.parent.currentRank);
            rank.AuthorityOver.RemoveAll(c=>c.flag_or_command.ToLower() == s.ToLower());
            if(i != 0)
                rank.AuthorityOver.Add(new Rank.Permission{
                    flag_or_command = s,
                    access = i == 1 ? Rank.Permission.Access.Allow : Rank.Permission.Access.Deny
                });
            Rank.SetRankHasAuthority(parent.parent.currentRank, s, i);
        };

        var rank = Rank.FromName(parent.parent.currentRank);
        if (rank is null)return;

        foreach(var group in Command.commands.Values.OrderBy(c=>c.Name).GroupBy(c=>c.Category).OrderBy(c=>c.Key)){
            Commands.Add.Label(group.Key, "groupHeader");
            foreach(var command in group)
                Commands.AddChild(new PermissionButton(this, command.Name, hasCommand, setCommand, commandInherited));
        }
        foreach(var command in AdminCore.AllFlags()){
            Flags.AddChild(new PermissionButton(this, command, hasFlag, setFlag, flagInherited));
        }
        foreach(var r in AdminCore.ranks.OrderBy(c=>-c.Authority)){
            Authorities.AddChild(new PermissionButton(this, r.Name, hasAuthority, setAuthority, authorityInherited));
        }

        /*var rank = Rank.FromName(parent.parent.currentRank);
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
        }*/
    }
    class PermissionList : Panel {
        public PermissionList(){
            AddClass("permissionList");
        }
    }

    class PermissionButton : Panel {
        public string cmd;
        public Button denied;
        public Button inherited;
        public Button allowed;
        public Label name;
        Permissions page;

        public Action<string, int> setHas;
        public Func<string, bool> getHas;
        public Func<string, bool> isInherited;

        public PermissionButton(Permissions page, string cmd, Func<string, bool> getHas, Action<string, int> setHas, Func<string, bool> isInherited){
            this.getHas = getHas;
            this.setHas = setHas;
            this.isInherited = isInherited;
            this.cmd = cmd;

            this.page = page;
            AddClass("permissionButton");
            SetClass("allowed", getHas(cmd));
            name = new Label{Text = cmd, Classes = "name"};
            denied = new("❌", "", ()=>{
                setHas(cmd, -1);
                page.UpdateAllButtons();
            });
            inherited = new("👪", "", ()=>{
                setHas(cmd, 0);
                page.UpdateAllButtons();
            });
            allowed = new("✔️", "", ()=>{
                setHas(cmd, 1);
                page.UpdateAllButtons();
            });

            AddChild(name);
            AddChild(denied);
            AddChild(inherited);
            AddChild(allowed);

            UpdateButtons();
        }

        public void UpdateButtons(){
            denied.SetClass("active", !isInherited(cmd)&&!getHas(cmd));
            inherited.SetClass("active", isInherited(cmd));
            allowed.SetClass("active", !isInherited(cmd)&&getHas(cmd));
        }
    }

    /*class AuthorityButtonInherit : AuthorityButton {
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
    }*/
}