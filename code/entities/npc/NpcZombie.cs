using Sandbox;
using System.Linq;

[Library( "npc_zombie", Title = "Zombie", Spawnable = true, Group = "NPC" )]
public partial class NpcZombie : NpcTest { 
    public override void Spawn(){
        base.Spawn();
        Health = 90;
        RenderColor = Color.Green;
    }

    
    [Net, Predicted]
    public bool attacking {get; set;} = false;
    public override void UpdateMovement(){
        SetAnimParameter("holdtype", 4);
        SetAnimParameter("holdtype_handedness", 0);
        var target = Entity.All.Where(x => x is SandboxPlayer p && p.LifeState == LifeState.Alive).OrderBy(x => x.Position.Distance(Position)).FirstOrDefault();
        if(target is not null){
            var dist = Position.Distance(target.Position);
            if(dist <= 80f){
                Steer = null;
                if(!attacking){
                    SetAnimParameter("b_attack", true);
                    attacking = true;
                    var t = target;
                    Do.After(0.3f, ()=>{
                        if(!t.IsValid() || !this.IsValid())
                            return;
                        if(Position.Distance(t.Position) < 80f){
                            AttackTarget(t);
                        }
                    });
                    Do.After(0.7f, ()=>{
                        attacking = false;
                    });
                }
            }else{
                Steer ??= new();
                if(Steer.Target.Distance(target.Position) > 30f)
                    Steer.Target = target.Position;
            }
        }
    }

    public virtual void AttackTarget( Entity target )
	{
        var strt = this.WorldSpaceBounds.Center;
        var end = target.WorldSpaceBounds.Center;
		var forward = (end - strt).Normal;

        var tr = Trace.Ray( strt, strt + forward * 5000 )
                .Ignore( Owner )
                .Ignore( this )
                .Size( 1 )
                .Run();
        
        tr.Surface.DoBulletImpact( tr );

        if ( !IsServer ) return;
        if ( !tr.Entity.IsValid() ) return;

        using ( Prediction.Off() )
        {
            var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * 10, 10 )
                .UsingTraceResult( tr )
                .WithAttacker( Owner )
                .WithWeapon( this );

            tr.Entity.TakeDamage( damageInfo );
        }
	}
}