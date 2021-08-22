using System.Linq;
using Sandbox.UI;
using Sandbox.UI.Construct;

class General : RankPanel.Page {
    public override string Name => "General";

    public General(RankPanel.RankSettings parent) : base(parent) {
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

        Add.Button("NOPE", "deleteButton", ()=>{
            parent.deleteButton.RemoveClass("yousure");
            parent.deleteButton.Text = "DELETE";
        });
        parent.deleteButton = Add.Button("DELETE", "deleteButton", ()=>{
            if(parent.deleteButton.HasClass("yousure")){
                AdminCore.ranks.RemoveAll(c=>c.Name.ToLower() == parent.parent.currentRank.ToLower());
                parent.parent.currentRanksPanel.Regenerate();
                parent.SetClass("inactive", true);
            }else{
                parent.deleteButton.AddClass("yousure");
                parent.deleteButton.Text = "DELETE: CAN'T UNDELETE";
            }
        });
    }
}