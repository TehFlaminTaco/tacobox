using Sandbox.UI.Construct;
using System;
using System.Linq;

namespace Sandbox.UI
{
	/// <summary>
	/// A horizontal slider with a text entry on the right
	/// </summary>
	public class ColorEditorHsva : Panel
	{
		public SliderEntry HueSlider { get; protected set; }
		public SliderEntry SaturationSlider { get; protected set; }
		public SliderEntry ValueSlider { get; protected set; }
		public SliderEntry AlphaSlider { get; protected set; }

		public ColorEditorHsva()
		{
			AddClass( "coloreditorhsva" );

			HueSlider = Add.SliderWithEntry( 0, 359, 0.1f );
			HueSlider.TextEntry.NumberFormat = "0";
			HueSlider.AddClass( "hue_slider" );
			HueSlider.Bind( "value", this, "HueValue" );

			SaturationSlider = Add.SliderWithEntry( 0, 1, 0.01f );
			SaturationSlider.TextEntry.NumberFormat = "0.00";
			SaturationSlider.AddClass( "saturation_slider" );
			SaturationSlider.Bind( "value", this, "SaturationValue" );

			ValueSlider = Add.SliderWithEntry( 0, 1, 0.01f );
			ValueSlider.TextEntry.NumberFormat = "0.00";
			ValueSlider.AddClass( "value_slider" );
			ValueSlider.Bind( "value", this, "ValueValue" );

			AlphaSlider = Add.SliderWithEntry( 0, 1, 0.01f );
			AlphaSlider.TextEntry.NumberFormat = "0.00";
			AlphaSlider.AddClass( "alpha_slider" );
			AlphaSlider.Bind( "value", this, "AlphaValue" );
		}

		ColorHsv color;

		/// <summary>
		/// The actual value. Setting the value will snap and clamp it.
		/// </summary>
		[Property]
		public ColorHsv Value 
		{
			get => color;
			set
			{
				if ( color == value ) return;

				color = value;

				_hueValue = value.Hue;
				_saturationValue = value.Saturation;
				_valueValue = value.Value;
				_alphaValue = value.Alpha;
				UpdateColors();
			}
		}

		void ColorChanged()
		{
			color = new ColorHsv( _hueValue, _saturationValue, _valueValue, _alphaValue );
			CreateValueEvent( "value", color.ToColor() );

			UpdateColors();
		}

		void UpdateColors()
		{
			var col = color.WithAlpha( 1 );

			SaturationSlider.Slider.Track.Style.Set( "background-image", $"linear-gradient( to right, {col.WithSaturation( 0 ).ToColor().Hex}, {col.WithSaturation( 1 ).ToColor().Hex} )" );
			ValueSlider.Slider.Track.Style.Set( "background-image", $"linear-gradient( to right, {col.WithValue( 0 ).ToColor().Hex}, {col.WithValue( 1 ).ToColor().Hex} )" );
		}

		public float _hueValue;

		public float HueValue
		{
			get => _hueValue;
			set
			{
				if ( _hueValue == value ) return;
				_hueValue = value;
				ColorChanged();
			}
		}

		public float _saturationValue;

		public float SaturationValue
		{
			get => _saturationValue;
			set
			{
				if ( _saturationValue == value ) return;
				_saturationValue = value;
				ColorChanged();
			}
		}

		public float _valueValue;

		public float ValueValue
		{
			get => _valueValue;
			set
			{
				if ( _valueValue == value ) return;
				_valueValue = value;
				ColorChanged();
			}
		}

		public float _alphaValue;

		public float AlphaValue
		{
			get => _alphaValue;
			set
			{
				if ( _alphaValue == value ) return;
				_alphaValue = value;
				ColorChanged();
			}
		}
	}
}
