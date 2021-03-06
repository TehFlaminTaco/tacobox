using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class ModelPicker : Panel {
    public string[] Models;
    private static readonly Regex fileShort = new(@".*/([^/.]*).*");
    
    public Func<string> GetSelectedModel;
    public Action<string> SetSelectedModel;

    List<(Panel panel, string model)> icons = new();
    public ModelPicker(string[] Models, Func<string> getter, Action<string> setter){
        if(Models is null)
            return;
        GetSelectedModel = getter;
        SetSelectedModel = setter;
        var scrollPanel = new Panel();
        scrollPanel.AddClass("box");
        this.Models = Models;
        foreach(var model in Models){
            if(!Local.Client.CanSpawnProp(model.Substring(7)))continue;
            var file = (string)model.Remove(model.Length - 5);
			var panel = scrollPanel.Add.Panel( "icon" );
            icons.Add((panel, model));
			panel.AddEventListener( "onclick", () => SetSelectedModel(model) );
            panel.Add.Label(fileShort.Replace(file, "$1"));
			//panel.Style.BackgroundImage = Texture.Load( $"{file}.vmdl_c.png", false );
        }
        AddChild(scrollPanel);
    }

    public override void Tick(){
        foreach((var panel, var model) in icons){
            panel.SetClass("selected", model == GetSelectedModel());
        }
    }
}