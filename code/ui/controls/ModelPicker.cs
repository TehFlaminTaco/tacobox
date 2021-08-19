using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

public class ModelPicker : Panel {
    public string[] Models;
    
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
            var file = (string)model.Remove(model.Length - 5);
            Log.Info(file);
			var panel = scrollPanel.Add.Panel( "icon" );
            icons.Add((panel, model));
			panel.AddEventListener( "onclick", () => SetSelectedModel(model) );
			panel.Style.Background = new PanelBackground
			{
				Texture = Texture.Load( $"{file}.vmdl_c.png", false )
			};
        }
        AddChild(scrollPanel);
    }

    public override void Tick(){
        foreach((var panel, var model) in icons){
            panel.SetClass("selected", model == GetSelectedModel());
        }
    }
}