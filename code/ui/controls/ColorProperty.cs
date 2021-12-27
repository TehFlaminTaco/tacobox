using Sandbox.UI.Construct;
using System;
using System.Linq;

namespace Sandbox.UI
{
	/// <summary>
	/// A horizontal slider. Can be float or whole number.
	/// </summary>
	public class ColorProperty : Panel
	{
		public Panel ColorSquare { get; protected set; }
		public TextEntry TextEntry { get; protected set; }

		public ColorProperty()
		{
			AddClass( "colorproperty" );

			ColorSquare = Add.Panel( "colorsquare" );
			ColorSquare.AddEventListener( "onmousedown", OpenPopup );

			TextEntry = Add.TextEntry( "#fff" );
			TextEntry.AddClass( "textentry" );
			TextEntry.Bind( "value", this, "TextValue" );

		}

		protected Color _value;

		/// <summary>
		/// The actual value. Setting the value will snap and clamp it.
		/// </summary>
		[Property]
		public Color Value 
		{
			get => _value;
			set
			{
				if ( _value == value ) return;

				_value = value;
				OnColorChanged( value );
			}
		}

		/// <summary>
		/// The actual value. Setting the value will snap and clamp it.
		/// </summary>
		[Property]
		public string TextValue
		{
			get => TextEntry.Text;
			set
			{
				var parsed = Color.Parse( value );
				if ( parsed.HasValue && parsed.Value != _value )
				{
					Value = value;
				}
			}
		}


		/// <summary>
		/// Convert a screen position to a value. The value is clamped, but not snapped.
		/// </summary>
		public virtual void OnColorChanged( Color color )
		{
			CreateValueEvent( "value", color );

			ColorSquare.Style.BackgroundColor = color;
			ColorSquare.Style.Dirty();

			if ( !TextEntry.HasActive )
			{
				var parsed = Color.Parse( TextEntry.Text );
				if ( !parsed.HasValue || parsed.Value != color )
				{
					TextEntry.Text = ColorToString( color );
				}
			}
		}

		public string ColorToString( Color color )
		{
			if ( color == Color.White ) return "white";
			if ( color == Color.Black ) return "black";
			if ( color == Color.Transparent ) return "transparent";

			if ( color.r <= 1 && color.g <= 1 && color.b <=1 )
			{
				byte r = Convert.ToByte( color.r * 255.0f );
				byte g = Convert.ToByte( color.g * 255.0f );
				byte b = Convert.ToByte( color.b * 255.0f );
				byte a = Convert.ToByte( color.a * 255.0f );

				if ( a == 255 )
				{
					return $"#{r:x2}{g:x2}{b:x2}";
				}
				else
				{
					return $"#{r:x2}{g:x2}{b:x2}{a:x2}";
				}
			}


			// TODO

			return color.Hex;
		}

		public virtual void OpenPopup()
		{
			var popup = new Popup( ColorSquare, Popup.PositionMode.BelowCenter, 32.0f );
			popup.AddClass( "medium" );
			popup.Title = "Color Picker";
			popup.Icon = "palette";

			var editor = popup.AddChild<ColorEditor>();
			editor.Bind( "value", this, "Value" );
		}

		[EditorProvider( typeof( Color ) )]
		public static Panel InspectorProvider( EditorProvider.Config config )
		{
			return new ColorProperty();
		}
	}

	namespace Construct
	{
		public static class ColorPropertyConstructor
		{
			public static ColorProperty ColorProperty( this PanelCreator self )
			{
				var control = self.panel.AddChild<ColorProperty>();

				return control;
			}
		}
	}
}
