using System.Collections.Generic;
using System.Linq;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class RankPanel : Panel {
    public string currentRank = "";
    public CurrentRanksPanel currentRanksPanel;
    public RankSettings rankSettings;
    public RankPanel(){
        StyleSheet.Load("ui/RanksMenu.scss");
        AddChild(new Window(this));

    }


    class Window : Panel {
        public RankPanel parent;
        public Window(RankPanel parent){
            this.parent = parent;
            AddClass("window");
            var closeButton = Add.Button("X", "closeButton", ()=>parent.Delete());
            AddChild(parent.currentRanksPanel = new(parent));
            AddChild(parent.rankSettings = new RankSettings(parent));
        }
    }

    public class CurrentRanksPanel : Panel {
        public RankPanel parent;
        public CurrentRanksPanel(RankPanel parent){
            this.parent = parent;
            AddClass("currentRanks");
            Regenerate();
        }

        public List<(string rank, Button button)> buttons;
        public void Regenerate(){
            DeleteChildren( true );
            buttons = new();
            Add.Button("Create New", "rank newRank", ()=>{
                Rank.MakeNewRank();
                if(Rank.FromName("New Rank") is null){ // Objectively bad way to do this. Shoot me.
                    AdminCore.ranks.Add(new Rank{
                        Name = "New Rank"
                    });
                }
                Regenerate();
            });
            foreach(var rank in AdminCore.ranks.OrderBy(r => -r.Authority)){
                var btn = Add.Button(rank.Name, "rank");
                btn.SetClass("selected", rank.Name.ToLower() == parent.currentRank.ToLower());
                btn.Style.FontColor = Color.Parse(rank.NameColor)??Color.White;
                btn.Style.Dirty();
                btn.AddEventListener("onclick", ()=>{
                    parent.currentRank = rank.Name;
                    foreach(var rb in buttons){
                        rb.button.SetClass("selected", rb.rank.ToLower() == parent.currentRank.ToLower());
                    }
                    parent.rankSettings.RankChanged();
                });
                buttons.Add((rank.Name, btn));
            }
        }
    }

    public class RankSettings : Panel {
        public RankPanel parent;
        List<(Button button, Page page)> pageButtons;
        List<Page> pages;
        public string CurrentPage = "General";

        public TextEntry rankName;
        public DropDown inherited;
        public SliderEntry authority;
        public ColorEditor colorEditor;
        public ColorEditorRgba colorEditorRgba;
        public Button deleteButton;
        public Permissions permissionsSettings;
        public SpawnPage spawnSettings;
        public PropsAllowed propsAllowed;

        public RankSettings(RankPanel parent){
            this.parent = parent;
            AddClass("rankSettings");
            pages = new();
            pageButtons = new();

            var pageButtonRow = new Panel();
            pageButtonRow.AddClass("pageButtons");
            var pageHolder = new Panel();
            pageHolder.AddClass("pageHolder");


            var generalSettings = new General(this);
            pages.Add(generalSettings);
            pageButtons.Add((pageButtonRow.Add.Button(generalSettings.Name, "pageButton"), generalSettings));
            pageHolder.AddChild(generalSettings);

            permissionsSettings = new(this);
            pages.Add(permissionsSettings);
            pageButtons.Add((pageButtonRow.Add.Button(permissionsSettings.Name, "pageButton"), permissionsSettings));
            pageHolder.AddChild(permissionsSettings);

            spawnSettings = new(this);
            pages.Add(spawnSettings);
            pageButtons.Add((pageButtonRow.Add.Button(spawnSettings.Name, "pageButton"), spawnSettings));
            pageHolder.AddChild(spawnSettings);

            propsAllowed = new(this);
            pages.Add(propsAllowed);
            pageButtons.Add((pageButtonRow.Add.Button(propsAllowed.Name, "pageButton"), propsAllowed));
            pageHolder.AddChild(propsAllowed);


            foreach(var pb in pageButtons){
                pb.button.AddEventListener("onclick", e=>{
                    CurrentPage = pb.page.Name;
                    UpdateSelectedPage();
                });
            }
            SetClass("inactive", true);
            UpdateSelectedPage();
            AddChild(pageButtonRow);
            AddChild(pageHolder);
        }

        public void UpdateSelectedPage(){
            foreach(var pb in pageButtons){
                pb.button.SetClass("currentPage", pb.page.Name == CurrentPage);
                pb.page.SetClass("hidden", pb.page.Name != CurrentPage);
            }
            deleteButton.Text = "DELETE";
            deleteButton.RemoveClass("yousure");
            if(CurrentPage == permissionsSettings.Name)permissionsSettings.UpdateChildren();
            if(CurrentPage == spawnSettings.Name)spawnSettings.UpdateChildren();
            if(CurrentPage == propsAllowed.Name)propsAllowed.UpdateChildren();
        }

        public void RankChanged(){
            SetClass("inactive", parent.currentRank is null);
            rankName.Text = parent.currentRank ?? "";
            rankName.SetClass("disabled", parent.currentRank.ToLower() == "guest");
            rankName.Blur();
            inherited.Options.Clear();
            inherited.Options.Add(new Option{
                Title = "None",
                Value = null
            });
            foreach(var rank in AdminCore.ranks.OrderBy(x=>-x.Authority)){
                if(Rank.FromName(parent.currentRank)?.WouldLoop(rank.Name)??false) continue;
                inherited.Options.Add(new Option{
                    Title = rank.Name,
                    Value = rank.Name
                });
            }
            inherited.Selected = inherited.Options.FirstOrDefault(x=>((x.Value as string)??"None")==(Rank.FromName(parent.currentRank)?.Inherit??"None")) ?? inherited.Options.First(x=>x.Title == "None");
            authority.Value = Rank.FromName(parent.currentRank)?.Authority ?? 0;
            colorEditor.Value = Color.Parse(Rank.FromName(parent.currentRank)?.NameColor??"white")??Color.White;
            colorEditorRgba.Value = Color.Parse(Rank.FromName(parent.currentRank)?.NameColor??"white")??Color.White;

            deleteButton.Text = "DELETE";
            deleteButton.RemoveClass("yousure");
            permissionsSettings.UpdateChildren();
            spawnSettings.UpdateChildren();
            propsAllowed.UpdateChildren();
        }
    }

    public abstract class Page : Panel {
        public abstract string Name {get;}
        public RankSettings parent;

        public Page(RankSettings parent){
            this.parent = parent;
            AddClass("tacopage");
            AddClass(Name);
        }
    }
}