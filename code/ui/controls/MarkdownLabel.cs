using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class MarkdownLabel : Panel {
    public static Regex markdownRegex = new Regex(@"
(?<bold>\*\*(?<body>.*?)\*\*|\b__(?<body>.*?)__\b|\[b](?<body>.*?)\[\/b])|
(?<italic>\*(?<body>.*?)\*|\b_(?<body>.*?)_\b|\[i](?<body>.*?)\[\/i])|
(?<strike>~~(?<body>.*?)~~|\[s](?<body>.*?)\[\/s])|
(?<color>\[colou?r=(?<colorcode>\#[0-9A-F]{3}|\#[0-9A-F]{6}|\w+)](?<body>.*?)\[\/colou?r])|
(?<noformat>.)", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

    private string _text = "";
    public string Text {
        get {return _text;}
        set {_text = value; Regenerate();}
    }
    public MarkdownLabel(string text, string classes) {
        AddClass(classes);
        Text = text;
    }

    public MarkdownLabel(string text) : this(text, ""){}

    //**bold** _small_ ~~strike~~ [color=red]RED[/color]
    public void Regenerate(){
        DeleteChildren( true );
        foreach(var chunk in FormatText(Text)){
            var txt = Add.Label(chunk.text);
            if((chunk.flavour&Flavour.Bold)!=Flavour.None)
                txt.Style.FontWeight=800;
            if((chunk.flavour&Flavour.Italic)!=Flavour.None)
                txt.Style.FontSize=10;
            if((chunk.flavour&Flavour.Strikethrough)!=Flavour.None)
                txt.Style.TextDecoration=TextDecoration.LineThrough;
            txt.Style.FontColor = chunk.color;

            txt.Style.Dirty();
        }
    }

    List<FlavourText> FormatText(string text, List<FlavourText> result, Flavour flavour=Flavour.None, Color? color=null) {
        if(text.Length <= 0)return result;
        var concurrentText = "";
        foreach(Match match in markdownRegex.Matches(text)){
            if(match.Groups["noformat"].Value.Length>0){
                concurrentText += match.Groups["noformat"].Value;
            }else{
                if(concurrentText.Length > 0){
                    result.Add(new FlavourText{
                        text = concurrentText,
                        flavour = flavour,
                        color = color??Color.White
                    });
                    concurrentText = "";
                }
                     if(match.Groups["bold"].Value.Length > 0)FormatText(match.Groups["body"].Value, result, flavour | Flavour.Bold, color);
                else if(match.Groups["italic"].Value.Length > 0)FormatText(match.Groups["body"].Value, result, flavour | Flavour.Italic, color);
                else if(match.Groups["strike"].Value.Length > 0)FormatText(match.Groups["body"].Value, result, flavour | Flavour.Strikethrough, color);
                else if(match.Groups["color"].Value.Length > 0)FormatText(match.Groups["body"].Value, result, flavour, Color.Parse(match.Groups["colorcode"].Value)??color);
            }
        }
        if(concurrentText.Length > 0){
            result.Add(new FlavourText{
                text = concurrentText,
                flavour = flavour,
                color = color??Color.White
            });
            concurrentText = "";
        }
        return result;
    }
    List<FlavourText> FormatText(string text){
        return FormatText(text, new());
    }

    class FlavourText {
        public string text = "";
        public Flavour flavour = Flavour.None;
        public Color color = Color.White;
    }

    enum Flavour {
        None=0,
        Bold=1,
        Italic=2,
        Strikethrough=4
        
    }
}