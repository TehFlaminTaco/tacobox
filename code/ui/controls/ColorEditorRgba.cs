using Sandbox.UI.Construct;
using System;
using System.Linq;

namespace Sandbox.UI
{
	/// <summary>
	/// A horizontal slider with a text entry on the right
	/// </summary>
	public class ColorEditorRgba : Panel
	{
		public SliderEntry RedSlider { get; protected set; }
		public SliderEntry GreenSlider { get; protected set; }
		public SliderEntry BlueSlider { get; protected set; }
		public SliderEntry AlphaSlider { get; protected set; }

		public ColorEditorRgba()
		{
			AddClass( "coloreditorrgba" );

			RedSlider = Add.SliderWithEntry( 0, 255, 1 );
			RedSlider.AddClass( "red_slider" );
			RedSlider.Bind( "value", this, "RedValue" );

			GreenSlider = Add.SliderWithEntry( 0, 255, 1 );
			GreenSlider.AddClass( "green_slider" );
			GreenSlider.Bind( "value", this, "GreenValue" );

			BlueSlider = Add.SliderWithEntry( 0, 255, 1 );
			BlueSlider.AddClass( "blue_slider" );
			BlueSlider.Bind( "value", this, "BlueValue" );

			AlphaSlider = Add.SliderWithEntry( 0, 255, 1 );
			AlphaSlider.AddClass( "alpha_slider" );
			AlphaSlider.Bind( "value", this, "AlphaValue" );
		}

		Color color;

		/// <summary>
		/// The actual value. Setting the value will snap and clamp it.
		/// </summary>
		[Property]
		public Color Value 
		{
			get => color;
			set
			{
				if ( color == value ) return;

				color = value;
				CreateValueEvent( "value", color );
			}
		}

		public float RedValue
		{
			get => Value.r * 255;
			set 
			{
				Value = Value.WithRed( value / 255.0f );
			}
		}

		public float GreenValue
		{
			get => Value.g * 255;
			set
			{
				Value = Value.WithGreen( value / 255.0f );
			}
		}

		public float BlueValue
		{
			get => Value.b * 255;
			set
			{
				Value = Value.WithBlue( value / 255.0f );
			}
		}

		public float AlphaValue
		{
			get => Value.a * 255;
			set
			{
				Value = Value.WithAlpha( value / 255.0f);
			}
		}
	}
}
