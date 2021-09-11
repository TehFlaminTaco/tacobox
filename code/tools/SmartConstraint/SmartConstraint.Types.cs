using System;
using System.Collections.Generic;
using Sandbox;

public partial class SmartConstraintTool {
    enum ConstraintType {
        Weld,
        ParentWeld,
        Move
    }

    Dictionary<ConstraintType, Action> ConstraintMethods => new Dictionary<ConstraintType, Action>{
        [ConstraintType.Weld] = ()=>{
            var oldPos = oldPosition;
            var oldRot = oldRotation;
            var ent = targetBody;

            var lockedAngle = spinAmount;
            if(SnapRotation){
                lockedAngle = (float)Math.Round(lockedAngle / SnapDegrees) * SnapDegrees;
            }

            var weld = PhysicsJoint.Weld
                .From(connectBody, connectLocalPosition, Rotation.LookAt(-connectLocalNormal + connectLocalNormal*Offset).RotateAroundAxis(Vector3.Forward, lockedAngle))
                .To(targetBody, targetLocalPosition, Rotation.LookAt(targetLocalNormal)).Create();
            (Owner as SandboxPlayer).undoQueue.Add(new UndoGeneric("Undid Weld",
                ()=>true,
                ()=>{
                    weld.Remove();
                    ent.Position = oldPos;
                    ent.Rotation = oldRot;
                }
            ));
        },
        [ConstraintType.ParentWeld] = ()=>{
            var oldPos = oldPosition;
            var oldRot = oldRotation;
            var ent = targetBody;

            var lockedAngle = spinAmount;
            if(SnapRotation){
                lockedAngle = (float)Math.Round(lockedAngle / SnapDegrees) * SnapDegrees;
            }
            if(!(targetBody.Entity is Prop targetProp && connectBody.Entity is Prop connectProp)){
                return;
            }
            targetProp.Weld(connectProp);
            (Owner as SandboxPlayer).undoQueue.Add(new UndoGeneric("Undid Parent Weld",
                ()=>true,
                ()=>{
                    targetProp.Unweld(true);
                    ent.Position = oldPos;
                    ent.Rotation = oldRot;
                }
            ));
        },
        [ConstraintType.Move] = ()=>{
            var oldPos = oldPosition;
            var oldRot = oldRotation;
            var ent = targetBody;
            (Owner as SandboxPlayer).undoQueue.Add(new UndoGeneric("Undid Move",
                ()=>true,
                ()=>{
                    ent.Position = oldPos;
                    ent.Rotation = oldRot;
                }
            ));
        }
    };
}