using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Sandbox.UI
{
	public class EditorProvider : LibraryMethod
	{
		public Type TargetType { get; set; }
		public string TargetName { get; set; }
		public bool PreferNoLabel { get; set; }

		public EditorProvider( Type t )
		{
			TargetType = t;
		}

		public EditorProvider( string targetName )
		{
			TargetName = targetName;
		}

		public struct Config
		{
			public object Value;
			public PropertyInfo Property;
		}

		public Panel CreateEditor( object value, PropertyInfo property = null )
		{
			var args = new Config
			{
				Value = value,
				Property = property
			};

			return InvokeStatic( args ) as Panel;
		}

		/// <summary>
		/// Try to get an appropriate inspector for this type
		/// </summary>
		public static bool TryGetForType( Type t, out EditorProvider provider, PropertyInfo property = null, string fallback = "default" )
		{
			var types = Library.GetAttributes<EditorProvider>().ToArray();

			//
			// First of all, does this speficy an editor to use?
			//
			if ( property != null )
			{
				var editors = property.GetCustomAttributes<EditorAttribute>();
				if ( editors != null )
				{
					foreach ( var editor in editors )
					{
						// we only want editors that are compatible with our Panel system
						if ( editor.EditorBaseTypeName != "Panel" ) continue;

						provider = types.Where( x => x.TargetName == editor.EditorTypeName ).FirstOrDefault();
						if ( provider != null )
							return true;
					}
				}
			}

			//
			// Exact type
			//
			provider = types.Where( x => x.TargetType != null && x.TargetType == t ).FirstOrDefault();
			if ( provider != null )
				return true;

			//
			// Roughly good type
			//
			provider = types.Where( x => x.TargetType != null && x.TargetType.IsAssignableFrom( t ) ).FirstOrDefault();
			if ( provider != null )
				return true;

			//
			// Default mode
			//
			if ( fallback != null )
			{
				provider = types.FirstOrDefault( x => x.TargetName == fallback );
				if ( provider != null )
					return true;
			}

			return false;
		}

	}
}
