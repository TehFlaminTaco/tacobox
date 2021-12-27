using Sandbox.UI.Construct;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sandbox.UI
{

	/// <summary>
	/// A horizontal slider. Can be float or whole number.
	/// </summary>
	public class Inspector : Form
	{

		[Property]
		public object Target { get; set; }

		[Property]
		public bool Recursive { get; set; } = true;		

		public Inspector()
		{
			AddClass( "inspector" );
		}

		int lastHash;

		public override void Tick()
		{
			if ( Target is IValid valid && !valid.IsValid )
				Target = null;

			if ( HashCode.Combine( Target, Recursive ) != lastHash )
			{
				Rebuild();
			}

			base.Tick();
		}



		public virtual void Rebuild()
		{
			DeleteChildren( true );
			lastHash = HashCode.Combine( Target, Recursive );

			if ( Target == null )
				return;

			// Get the Class Info
			var properties = Reflection.GetProperties( Target );
			if ( properties == null ) throw new System.Exception( "Oops" );

			// Make a field for each property
			foreach ( var group in properties.GroupBy( x => GetCategory( x ) ).OrderBy( x => x.Key ) )
			{
				AddHeader( group.Key );

				currentGroup = Add.Panel( "field-group" );

				foreach ( var prop in group.OrderBy( x => x.Name ) )
				{
					if ( !Recursive && prop.DeclaringType != Target.GetType() )
						continue;

					if ( prop.GetGetMethod() == null )
						continue;

					CreateControlFor( Target, prop );
				}

				currentGroup = null;
			}
		}

		private string GetCategory( MemberInfo prop )
		{
			var category = prop.GetCustomAttribute<CategoryAttribute>();
			if ( category != null ) return category.Category;

			return "Misc";
		}


		public virtual void CreateControlFor( object obj, PropertyInfo prop )
		{
			var browsable = prop.GetCustomAttribute<BrowsableAttribute>();
			if ( !(browsable?.Browsable ?? true) ) return;

			if ( EditorProvider.TryGetForType( prop.PropertyType, out var handler, property: prop ) )
			{
				var control = handler.CreateEditor( prop.GetValue( obj ), prop );
				if ( control != null )
				{
					if ( handler.PreferNoLabel )
					{
						(currentGroup ?? this).AddChild( control );
					}
					else
					{
						AddRow( prop, Target, control );
					}
					
					return;
				}
			}

			AddRow( prop, Target, new TextEntry() );
		}


		[EditorProvider( typeof( float ) )]
		public static Panel CreateNumericControl( EditorProvider.Config config )
		{
			var range = config.Property?.GetCustomAttribute<RangeAttribute>();
			if ( range != null )
			{
				var slider = new SliderEntry();
				slider.MinValue = range.Min;
				slider.MaxValue = range.Max;
				slider.Step = range.Step;
				return slider;
			}

			var te = new TextEntry();
			te.Numeric = true;
			te.NumberFormat = "0.###";
			return te;
		}

		[EditorProvider( typeof( System.Enum ) )]
		public static Panel CreateEnumControl( EditorProvider.Config config )
		{
			var control = new DropDown();

			var names = config.Property?.PropertyType.GetEnumNames();
			var values = config.Property?.PropertyType.GetEnumValues();

			for ( int i = 0; i < names.Length; i++ )
			{
				control.Options.Add( new Option(){Title =names[i ], Value=values.GetValue( i ).ToString()} );
			}

			return control;
		}

		[EditorProvider( typeof( int ) )]
		public static Panel CreateIntegerControl( EditorProvider.Config config )
		{
			var range = config.Property?.GetCustomAttribute<RangeAttribute>();
			if ( range != null )
			{
				var slider = new SliderEntry();
				slider.MinValue = range.Min;
				slider.MaxValue = range.Max;
				slider.Step = range.Step;

				if ( config.Property.PropertyType == typeof( int ) || config.Property.PropertyType == typeof( uint ) )
				{
					slider.TextEntry.NumberFormat = "0.";
					slider.Slider.Step = 1;
				}
	
				return slider;
			}

			var te = new TextEntry();
			te.Numeric = true;
			te.NumberFormat = "0";
			return te ;
		}

		[EditorProvider( typeof( bool ) )]
		public static Panel CreateBooleanControl( EditorProvider.Config config )
		{
			return new Checkbox();
		}

		[EditorProvider( typeof( string ) )]
		public static Panel CreateStringControl( EditorProvider.Config config )
		{
			return new TextEntry();
		}

		[EditorProvider( "default" )]
		public static Panel CreateDefaultControl( EditorProvider.Config config )
		{
			return new TextEntry();
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			Rebuild(); 
		}

	}
}
