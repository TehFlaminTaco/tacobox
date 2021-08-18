using System.Collections.Generic;
using Sandbox;

interface IWireEntity {
	public List<WireVal> Values();

	public string Name() {
		return (this as Entity)?.ClassInfo.Title;
	}
}