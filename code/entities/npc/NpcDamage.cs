using System;
using System.Threading;
using Sandbox;

public partial class NpcTest {
    public override void TakeDamage( DamageInfo info ){
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 10.0f;
		}

		Health -= info.Damage;

		if(Health <= 0){
			Die(info);
		}

		//base.TakeDamage( info );
	}

	public void Die(DamageInfo info){
        var ent = new Prop();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.Scale = Scale;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		//ent.CollisionGroup = CollisionGroup.Debris;
		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.TakeDecalsFrom( this );
		ent.EnableHitboxes = false;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColorAndAlpha = RenderColorAndAlpha;
		try{
			ent.PhysicsGroup.Velocity = PhysicsBody.Velocity;
			var bone = ent.GetBonePhysicsBody(info.BoneIndex);
			if(bone is null || !bone.IsValid())
				bone = ent.PhysicsBody;
			if(bone is not null && bone.IsValid())
				bone.ApplyImpulse(info.Force * 50f);
		}catch(Exception){}
		ent.PhysicsGroup.RebuildMass();
        
        ent.PhysicsGroup.SetSurface("flesh");
        for(int i=0; i < this.BoneCount; i++){
            this.GetBonePhysicsBody(i)?.PhysicsGroup?.SetSurface("flesh");
        }

		if ( Local.Pawn == this )
		{
			//ent.EnableDrawing = false; wtf
		}

		ent.SetInteractsAs( CollisionLayer.All );
		//ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		//ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		foreach ( var child in Children )
		{
			if ( child is ModelEntity e )
			{
				var model = e.GetModelName();
				if ( model != null && !model.Contains( "clothes" ) )
					continue;

				var clothing = new ModelEntity();
				clothing.SetModel( model );
				clothing.SetParent( ent, true );
				clothing.RenderColorAndAlpha = e.RenderColorAndAlpha;
                clothing.PhysicsBody?.PhysicsGroup?.SetSurface("flesh");

				if ( Local.Pawn == this )
				{
					//	clothing.EnableDrawing = false; wtf
				}
			}
		}

        Do.After(8.0f, ()=>ent.Delete());

        Delete();
    }
}