using System;
using Sandbox;
using Sandbox.Tools;

[Library( "tool_smartconstraint", Title = "Smart Constraint", Description = "Do various things to combine two things", Group = "construction" )]
public partial class SmartConstraintTool : BaseTool {

    [Net] PhysicsBody targetBody {get; set;} = null;
    Vector3 targetLocalPosition = Vector3.Zero;
    Vector3 targetLocalNormal = Vector3.Zero;
    PhysicsBodyType targetBodyType = PhysicsBodyType.Static;

    Vector3 oldPosition = Vector3.Zero;
    Rotation oldRotation = Rotation.Identity;

    [Net] PhysicsBody connectBody {get; set;} = null;
    Vector3 connectLocalPosition = Vector3.Zero;
    Vector3 connectLocalNormal = Vector3.Zero;

    [Net] public bool DoEyeLock {get; set;} = false;

    public float spinAmount = 0.0f;


    public override void Simulate()
    {
        if ( !Host.IsServer )
            return;

        using ( Prediction.Off() )
        {
            if(!this.CanTool())return;

            var tr = (Owner as SandboxPlayer).EyeTrace();

            if(Host.IsServer)ValidateBodies();

            if(targetBody is not null && connectBody is not null)
                MoveBody();

            if(Input.Pressed( InputButton.Attack1 ))
                Attack1(tr);
            
            if(Input.Pressed( InputButton.Attack2 ))
                Attack2(tr);
            
            if(Input.Pressed( InputButton.Reload ))
                Reload(tr);
        }
    }

    public void Attack1(TraceResult tr){
        CreateHitEffects( tr.EndPos );

        var isBody = true;
        if ( !tr.Hit || !tr.Body.IsValid() || !tr.Entity.IsValid() || tr.Entity.IsWorld )
            isBody = false;

        if(targetBody is null){
            if(isBody)SetTarget(tr);
        }else if(connectBody is null){
            if(isBody)SetConnect(tr);
        }else
            SetRotation(tr);
    }

    public void Attack2(TraceResult tr){
        CreateHitEffects( tr.EndPos );
        if(targetBody is null){
            Nudge(tr, NudgeAmount);
        }
    }

    public void Reload(TraceResult tr){
        CreateHitEffects( tr.EndPos );
        if(targetBody is null){
            Nudge(tr, -NudgeAmount);
        }else{
            targetBody = null;
            connectBody = null;
        }
    }

    public void Nudge(TraceResult tr, float amount){
        if(tr.Entity is ModelEntity m && !m.IsWorld){
            var dir = -tr.Normal;
            if(NudgePercent){
                var localDir = m.Transform.NormalToLocal(dir);
                var size = m.GetModel().Bounds.Maxs - m.GetModel().Bounds.Mins;
                amount = Math.Abs(size.Dot(localDir)) * (amount/100f);
            }
            m.Position += dir * amount;
        }
    }

    public void SetTarget(TraceResult tr){
        targetBody = tr.Body;
        targetLocalPosition = tr.Body.Transform.PointToLocal(tr.EndPos);
        targetLocalNormal = tr.Body.Transform.NormalToLocal(tr.Normal);
        targetBodyType = tr.Body.BodyType;
    }

    public void SetConnect(TraceResult tr){
        if(targetBody == tr.Body)return;
        connectBody = tr.Body;
        connectLocalPosition = tr.Body.Transform.PointToLocal(tr.EndPos);
        connectLocalNormal = tr.Body.Transform.NormalToLocal(tr.Normal);
        oldPosition = targetBody.Position;
        oldRotation = targetBody.Rotation;
        DoEyeLock = true;
    }

    public void SetRotation(TraceResult tr){
        _ = Enum.TryParse<ConstraintType>(ConstraintTypeName, out var typ);
        var methods = ConstraintMethods;
        if(methods.ContainsKey(typ)){
            methods[typ]();
        }
        targetBody.BodyType = targetBodyType;
        targetBody = null;
        connectBody = null;
        DoEyeLock = false;
    }

    public void MoveBody(){
        var targetRotation = Rotation.LookAt(targetLocalNormal);

        var lockedAngle = spinAmount;
        if(SnapRotation){
            lockedAngle = (float)Math.Round(lockedAngle / SnapDegrees) * SnapDegrees;
        }

        var connectRotation = Rotation.LookAt(-connectLocalNormal).RotateAroundAxis(Vector3.Forward, lockedAngle);
        if(DoRotation)targetBody.Rotation = connectBody.Rotation * Rotation.Difference(targetRotation, connectRotation);
        targetBody.Position += connectBody.Transform.PointToWorld(connectLocalPosition + connectLocalNormal*Offset) - targetBody.Transform.PointToWorld(targetLocalPosition);
        targetBody.Velocity = Vector3.Zero;
        targetBody.AngularVelocity = Vector3.Zero;
        if(targetBody.BodyType == PhysicsBodyType.Static){
            targetBody.BodyType = PhysicsBodyType.Dynamic;
        }
    }

    public void ValidateBodies(){
        if(targetBody is not null && !targetBody.IsValid()){
            targetBody = null;
            connectBody = null;
            DoEyeLock = false;
        }
        if(connectBody is not null && !connectBody.IsValid()){
            targetBody = null;
            connectBody = null;
            DoEyeLock = false;
        }
    }

    public override bool EyeLock(){
        if(DoEyeLock){
            if(Host.IsServer)
                spinAmount=Rotation.Difference(Input.Rotation, Owner.EyeRot).Angles().yaw;
            return true;
        }
        return false;
    }
}