using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

interface IWireEntity {
	public List<WireVal> Values();

	public string WireName() {
		return (this as Entity)?.ClassInfo.Title;
	}
}