using Sandbox.UI.Construct;
using Sandbox.UI.DataSource;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sandbox.UI
{
	/// <summary>
	/// A control for editing a list of something
	/// </summary>
	public class ListInspector : Panel
	{
		public ListInspector()
		{
			AddClass( "listinspector columns" );
		}

		IList _value;

		/// <summary>
		/// The actual value. Setting the value will snap and clamp it.
		/// </summary>
		[Property]
		public IList Value
		{
			get => _value;
			set
			{
				if ( _value == value ) return;

				_value = value;
				Rebuild();
			}
		}

		public void Rebuild()
		{
			DeleteChildren( true );

			int i = 0;
			foreach ( var val in _value )
			{
				var row = Add.Panel( "row" );
				var index = i;

				var controlpanel = row.Add.Panel( "control grow" );

				if ( EditorProvider.TryGetForType( val?.GetType(), out var handler ) )
				{
					var control = handler.CreateEditor( val );
					if ( control != null )
					{
						control.AddClass( "grow" );
						control.Bind( new ArraySource( "value", _value, index ) );
						controlpanel.AddChild( control );
						row.Add.ButtonWithIcon( null, "delete", "button-delete", () =>
						{
							_value.RemoveAt( index );
							Rebuild();
						} );
					}
				}

				i++;
			}

			var footer = Add.Panel( "row footer" );
			footer.Add.ButtonWithIcon( null, "add", "button-add", () =>
			{
				_value.Add( default );
				Rebuild();
			} );
		}
		public override void SetPropertyObject( string name, object value )
		{
			base.SetPropertyObject( name, value );
		}

		[EditorProvider( typeof( IList ) )]
		public static Panel CreateListControl( EditorProvider.Config config )
		{
			return new ListInspector();
		}
	}
}
