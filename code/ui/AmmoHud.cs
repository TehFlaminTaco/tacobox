using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

class AmmoHud : Panel
{
	public AmmoTypePanel ammo1;
	public AmmoTypePanel ammo2;

	protected static SandboxPlayer Player {
		get {
			return Local.Pawn as SandboxPlayer;
		}
	}

	protected static Weapon ActiveWeapon {
		get {
			if ( Local.Pawn is not SandboxPlayer player ) return null;
			return player.Inventory.Active as Weapon;
		}
	}

	public AmmoHud()
	{
		AddChild(ammo2 = new(new Ammo{
			valid = ()=>(ActiveWeapon?.Clip2Type??AmmoType.None)!=AmmoType.None,
			isPocket = ()=>ActiveWeapon?.Clip2Pocket??false,
			getPocket = ()=>Player.AmmoCount(ActiveWeapon?.Clip2Type??AmmoType.None),
			getClip = ()=>ActiveWeapon?.Clip2??0,
			getClipSize = ()=>ActiveWeapon?.Clip2Size??0
		}, "two"));
		AddChild(ammo1 = new(new Ammo{
			valid = ()=>(ActiveWeapon?.Clip1Type??AmmoType.None)!=AmmoType.None,
			isPocket = ()=>ActiveWeapon?.Clip1Pocket??false,
			getPocket = ()=>Player.AmmoCount(ActiveWeapon?.Clip1Type??AmmoType.None),
			getClip = ()=>ActiveWeapon?.Clip1??0,
			getClipSize = ()=>ActiveWeapon?.Clip1Size??0
		}, "one"));
	}

	public override void Tick()
	{
		ammo1.Tick();
		ammo2.Tick();
	}

	public class AmmoTypePanel : Panel {
		ClipPanel clip;
		public Label pocket;
		public Ammo ammo;
		public AmmoTypePanel(Ammo ammo, string classes=""){
			this.ammo = ammo;
			this.Classes = classes;
			this.AddClass("ammopanel");

			AddChild(clip = new(ammo, classes));
			pocket = Add.Label("000", classes);
			pocket.AddClass("pocket");
		}

		public override void Tick(){
			SetClass("hidden", !ammo.valid());
			pocket.Text = $"{ammo.getPocket()}".PadLeft(3, '0');
			if(pocket.Text.Length > 3){
				pocket.Text = "+999";
			}
			bool isPocket = ammo.isPocket();
			pocket.SetClass("pocketonly", isPocket);
			clip.SetClass("hidden", isPocket);
			if(!isPocket)clip.Tick();
		}
	}

	public class ClipPanel : Panel {
		Label inClip;
		Label clipSize;
		Ammo ammo;
		public ClipPanel(Ammo ammo, string classes=""){
			this.ammo = ammo;
			this.Classes = classes;
			this.AddClass("clippanel");
		 	inClip = Add.Label("0", classes);
			inClip.AddClass("clip");
		 	clipSize = Add.Label("/0", classes);
			clipSize.AddClass("clipsize");
		}

		public override void Tick(){
			var iclip = ammo.getClip();
			var iclipSize = ammo.getClipSize();
			clipSize.SetClass("hidden", iclipSize==0);
			if(ammo.valid()){
				if(iclipSize>0)
					clipSize.Text = $"/{iclipSize}";
				inClip.Text = $"{iclip}";
			}
		}
	}

	public class Ammo{
		public Func<bool> valid;
		public Func<bool> isPocket;
		public Func<int> getPocket;
		public Func<int> getClip;
		public Func<int> getClipSize;
	}
}
