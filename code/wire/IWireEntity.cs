using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

interface IWireEntity {
	public List<WireVal> Values();

	public string Name() {
		return (this as Entity)?.ClassInfo.Title;
	}
}