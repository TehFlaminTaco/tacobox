using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public abstract class Perms : RankPanel.Page {
    public ListType List1;
    public ListType List2;
    public ListType List3;

    public abstract void MakeLists();

    public abstract class ListType {
        public PermList list;
        public abstract string Name {get;}
        public abstract void Make();
        public abstract bool Has(string rankName, string cmd);
        public abstract bool Inherited(string rankName, string cmd);
        public abstract void Set(string rankName, string cmd, int setting);

        public void Add(string name){
            list.AddChild(new PermButton(list.page, name, this.Has, this.Set, this.Inherited));
        }
    }

    public Perms(RankPanel.RankSettings parent) : base(parent) {
        AddClass("perms");
        Form form = new Form();
        AddChild(form);
        MakeLists();

        if(List1 is not null)form.AddRow(List1.Name, List1.list = new(this));
        if(List2 is not null)form.AddRow(List2.Name, List2.list = new(this));
        if(List3 is not null)form.AddRow(List3.Name, List3.list = new(this));

        UpdateChildren();
    }

    public void UpdateAllButtons(){
        var allLists = Enumerable.Empty<PermButton>();
        if(List1 is not null)allLists = allLists.Union(List1.list.ChildrenOfType<PermButton>());
        if(List2 is not null)allLists = allLists.Union(List2.list.ChildrenOfType<PermButton>());
        if(List3 is not null)allLists = allLists.Union(List3.list.ChildrenOfType<PermButton>());
        foreach(var btn in allLists){
            btn.SetClass("allowed", btn.getHas(parent.parent.currentRank, btn.cmd));
            btn.UpdateButtons();
        }
        
    }

    public void UpdateChildren(){
        List1?.list.DeleteChildren( true );
        List2?.list.DeleteChildren( true );
        List3?.list.DeleteChildren( true );

        var rank = Rank.FromName(parent.parent.currentRank);
        if (rank is null)return;

        List1?.Make();
        List2?.Make();
        List3?.Make();
    }
    public class PermList : Panel {
        public Perms page;
        public PermList(Perms page){
            this.page = page;
            AddClass("permsList");
        }
    }

    public class PermButton : Panel {
        public string cmd;
        public Button denied;
        public Button inherited;
        public Button allowed;
        public Label name;
        Perms page;

        public Action<string, string, int> setHas;
        public Func<string, string, bool> getHas;
        public Func<string, string, bool> isInherited;

        public PermButton(Perms page, string cmd, Func<string, string, bool> getHas, Action<string, string, int> setHas, Func<string, string, bool> isInherited){
            this.getHas = getHas;
            this.setHas = setHas;
            this.isInherited = isInherited;
            this.cmd = cmd;

            this.page = page;
            var rank = page.parent.parent.currentRank;
            AddClass("permsButton");
            SetClass("allowed", getHas(rank, cmd));
            name = new Label{Text = cmd, Classes = "name"};
            denied = new("❌", "", ()=>{
                setHas(rank, cmd, -1);
                page.UpdateAllButtons();
            });
            inherited = new("👪", "", ()=>{
                setHas(rank, cmd, 0);
                page.UpdateAllButtons();
            });
            allowed = new("✔️", "", ()=>{
                setHas(rank, cmd, 1);
                page.UpdateAllButtons();
            });

            AddChild(name);
            AddChild(denied);
            AddChild(inherited);
            AddChild(allowed);

            UpdateButtons();
        }

        public void UpdateButtons(){
            var rank = page.parent.parent.currentRank;
            denied.SetClass("active", !isInherited(rank,cmd)&&!getHas(rank,cmd));
            inherited.SetClass("active", isInherited(rank,cmd));
            allowed.SetClass("active", !isInherited(rank,cmd)&&getHas(rank,cmd));
        }
    }
}