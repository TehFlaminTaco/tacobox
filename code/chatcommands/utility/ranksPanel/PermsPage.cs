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
        public virtual bool IsInt => false;
        public PermList list;
        public abstract string Name {get;}
        public abstract void Make();
        public abstract int Value(string rankName, string cmd);
        public abstract bool Inherited(string rankName, string cmd);
        public abstract void Set(string rankName, string cmd, int setting);

        public void Add(string name){
            list.AddChild(new PermButton(list.page, name, this));
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
            btn.SetClass("allowed", btn.getValue(parent.parent.currentRank, btn.cmd)!=0);
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
        ListType list;
        public string cmd;
        public Button denied;
        public Button inherited;
        public Button allowed;
        public Button unlimited;
        public TextEntry count;
        public Label name;
        Perms page;

        public Action<string, string, int> setHas;
        public Func<string, string, int> getValue;
        public Func<string, string, bool> isInherited;

        public PermButton(Perms page, string cmd, ListType list){
            this.list = list;

            this.getValue = list.Value;
            this.setHas = list.Set;
            this.isInherited = list.Inherited;
            this.cmd = cmd;

            this.page = page;
            var rank = page.parent.parent.currentRank;
            AddClass("permsButton");
            SetClass("allowed", getValue(rank, cmd)>0);
            name = new Label{Text = cmd, Classes = "name"};
            if(list.IsInt){
                inherited = new("👪", "", ()=>{
                    setHas(rank, cmd, -2);
                    page.UpdateAllButtons();
                });
                unlimited = new("∞", "", ()=>{
                    setHas(rank, cmd, -1);
                    page.UpdateAllButtons();
                });
                count = new TextEntry{
                    Text = ""+getValue(rank,cmd),
                    Numeric = true
                };
                count.AddEventListener("value.changed", e=>{
                    setHas(rank, cmd, count.Text.ToInt());
                    page.UpdateAllButtons();
                });

                AddChild(name);
                AddChild(inherited);
                AddChild(unlimited);
                AddChild(count);

            }else{
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
            }

            UpdateButtons();
        }

        public void UpdateButtons(){
            var rank = page.parent.parent.currentRank;
            if(list.IsInt){
                inherited.SetClass("active", isInherited(rank,cmd));
                unlimited.SetClass("active", !isInherited(rank,cmd)&&getValue(rank,cmd)==-1);
            }else{
                denied.SetClass("active", !isInherited(rank,cmd)&&getValue(rank,cmd)==0);
                inherited.SetClass("active", isInherited(rank,cmd));
                allowed.SetClass("active", !isInherited(rank,cmd)&&getValue(rank,cmd)>0);
            }
        }
    }
}