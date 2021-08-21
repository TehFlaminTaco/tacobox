using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class CommandRanks : Command {
    public override string Name => "Ranks";
    public override string Category => "Utility";

    public override bool Run(Player executor, IEnumerable<string> args){
        //executor.TakeDamage(DamageInfo.Generic(900000f));
        //ChatBox.AddChatEntry(To.Everyone, "", $"⚠️ {executor.GetClientOwner().Name} killed themself."); //avatar:{executor.GetClientOwner().SteamId}
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

        public Commands commandsSettings;
        public Flags flagsSettings;
        public Authoritys authoritysSettings;

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

            commandsSettings = new Commands(this);
            pages.Add(commandsSettings);
            pageButtons.Add((pageButtonRow.Add.Button(commandsSettings.Name, "pageButton"), commandsSettings));
            pageHolder.AddChild(commandsSettings);

            flagsSettings = new Flags(this);
            pages.Add(flagsSettings);
            pageButtons.Add((pageButtonRow.Add.Button(flagsSettings.Name, "pageButton"), flagsSettings));
            pageHolder.AddChild(flagsSettings);

            authoritysSettings = new Authoritys(this);
            pages.Add(authoritysSettings);
            pageButtons.Add((pageButtonRow.Add.Button(authoritysSettings.Name, "pageButton"), authoritysSettings));
            pageHolder.AddChild(authoritysSettings);


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
            flagsSettings.UpdateChildren();
            commandsSettings.UpdateChildren();
            authoritysSettings.UpdateChildren();
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

            flagsSettings.UpdateChildren();
            commandsSettings.UpdateChildren();
            authoritysSettings.UpdateChildren();
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
    class General : Page {
        public override string Name => "General";

        public General(RankSettings parent) : base(parent) {
            var form = new Form();
            form.AddClass("generalSettings");
            AddChild(form);

            parent.rankName = new TextEntry{
                Text = parent.parent.currentRank ?? ""
            };
            form.AddRow("Name", parent.rankName);
            parent.rankName.AddEventListener("onchange", e=>{
                parent.parent.currentRanksPanel.buttons.First(c=>c.button.Text.ToLower() == parent.parent.currentRank.ToLower()).button.Text = parent.rankName.Text;
                Rank.SetRankName(parent.parent.currentRank, parent.rankName.Text);
                parent.parent.currentRank = parent.rankName.Text;
            });

            parent.inherited = new DropDown();
            parent.inherited.Options.Add(new Option{
                Title = "None",
                Value = null
            });
            form.AddRow("Inherit From", parent.inherited);
            parent.inherited.AddEventListener("value.changed", e=>{
                Rank.SetRankInheritor(parent.parent.currentRank, parent.inherited.Selected.Value as string);
            });

            parent.authority = new SliderEntry{
                MinValue = 0,
                MaxValue = 999,
                Step = 1,
                Value = Rank.FromName(parent.parent.currentRank)?.Authority??0
            };
            form.AddRow("Authority", parent.authority);
            parent.authority.AddEventListener("value.changed", e=>{
                Rank.SetRankAuthority(parent.parent.currentRank, (int)parent.authority.Value);
            });

            parent.colorEditor = new ColorEditor{
                Value = Color.Parse(Rank.FromName(parent.parent.currentRank)?.NameColor??"white")??Color.White
            };
            parent.colorEditorRgba = new ColorEditorRgba{
                Value = Color.Parse(Rank.FromName(parent.parent.currentRank)?.NameColor??"white")??Color.White
            };
            parent.colorEditor.AddEventListener("value.changed", e=>{
                var btn = parent.parent.currentRanksPanel.buttons.FirstOrDefault(c=>c.button.Text.ToLower() == parent.parent.currentRank.ToLower()).button;
                if(btn is null)return;
                btn.Style.FontColor = parent.colorEditor.Value;
                btn.Style.Dirty();
                Rank.SetRankColor(parent.parent.currentRank, parent.colorEditor.Value.ToColor().Hex);
            });
            parent.colorEditorRgba.AddEventListener("value.changed", e=>{
                parent.colorEditor.Value = parent.colorEditorRgba.Value;
            });
            form.AddRow("Name Color", parent.colorEditor);
            form.AddRow("", parent.colorEditorRgba);
        }
    }
}