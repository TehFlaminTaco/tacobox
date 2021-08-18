using System;

namespace Sandbox.UI
{
	public class InspectorProvider : LibraryMethod
	{
		public Type TargetType { get; set; }

		public InspectorProvider( Type t )
		{
			TargetType = t;
		}

	}
}
