using Sandbox.UI.Construct;
using System;
using System.Linq;
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

		}

		int lastHash;

		public override void Tick()
		{
			base.Tick();

			var hash = HashCode.Combine( Target, Recursive );

			if ( hash != lastHash )
			{
				lastHash = hash;
				Rebuild();
			}			
		}

		public virtual void Rebuild()
		{
			DeleteChildren( true );

			// Get the Class Info
			var attr = Library.GetAttribute( Target.GetType() );
			if ( attr == null ) throw new System.Exception( "Oops" );

			var types = Library.GetAttributes<InspectorProvider>().ToArray(); // todo order by?

			// Make a field for each property
			foreach ( var prop in attr.Properties )
			{
				Panel control = default;

				if ( !Recursive && prop.DeclaringType != Target.GetType() ) 
					continue;

				// Try to create an editor from the attributes
				var handler = types.Where( x => x.TargetType == prop.PropertyType ).FirstOrDefault();
				control = (Panel)handler?.InvokeStatic( Target, prop );
				if ( control != null )
				{
					control.Parent = this;
				}
				else
				{
					// If we can't just do a textentry
					control = AddChild<TextEntry>();
				}

				// bind on value
				control.Bind( "value", Target, prop.MemberName );
				AddRow( prop.Title, control );
			}
		}


		[InspectorProvider( typeof( float ) )]
		public static Panel CreateNumericControl( object target, PropertyAttribute prop )
		{
			var range = prop.Attributes.OfType<RangeAttribute>().FirstOrDefault();
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
			te.Format = "0.###";
			return te;
		}

		[InspectorProvider( typeof( int ) )]
		public static Panel CreateIntegerControl( object target, PropertyAttribute prop )
		{
			var range = prop.Attributes.OfType<RangeAttribute>().FirstOrDefault();
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
			te.Format = "0";
			return te;
		}

		[InspectorProvider( typeof( bool ) )]
		public static Panel CreateBooleanrControl( object target, PropertyAttribute prop )
		{
			return new Checkbox();
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			Rebuild(); 
		}

	}
}
