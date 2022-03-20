using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public class SmartSnap {
    static bool DEBUG => false ;
    public static Vector3 LinePlaneIntersect(Vector3 rayOrigin, Vector3 rayDir, Vector3 planeOrigin, Rotation planeDown){
        var rayStart = planeOrigin-rayOrigin;
        rayStart *= planeDown.Inverse;
        rayDir *= planeDown.Inverse;

        var d = -rayStart.x/rayDir.x; // I think..?
        
        return new(0, -(rayStart.y + rayDir.y*d), -(rayStart.z + rayDir.z*d));
    }

    public static TraceResult SnapSurface(TraceResult eyeTrace){
        if(eyeTrace.Entity is ModelEntity m && !m.IsWorld){
            var pos = PointOnBox(m, eyeTrace.StartPosition, eyeTrace.Direction);
            if(pos is BoxFaceResult face){
                var EdgeColor = Color.Red;
                var MidColor = Color.Green;
                var QuartColor = Color.Yellow;
                DebugOverlay.Line(face.bottomLeft, face.bottomRight, EdgeColor);
                DebugOverlay.Line(face.bottomLeft, face.topLeft, EdgeColor);
                DebugOverlay.Line(face.topRight, face.topLeft, EdgeColor);
                DebugOverlay.Line(face.topRight, face.bottomRight, EdgeColor);

                DebugOverlay.Line((face.topLeft + face.topRight)/2f, (face.bottomLeft + face.bottomRight)/2f, MidColor);
                DebugOverlay.Line((face.topLeft + face.bottomLeft)/2f, (face.topRight + face.bottomRight)/2f, MidColor);

                DebugOverlay.Line((face.topLeft - face.topRight)*(1f/4f)+face.topRight, (face.bottomLeft - face.bottomRight)*(1f/4f)+face.bottomRight, QuartColor);
                DebugOverlay.Line((face.topLeft - face.bottomLeft)*(1f/4f)+face.bottomLeft, (face.topRight - face.bottomRight)*(1f/4f)+face.bottomRight, QuartColor);
                DebugOverlay.Line((face.topLeft - face.topRight)*(3f/4f)+face.topRight, (face.bottomLeft - face.bottomRight)*(3f/4f)+face.bottomRight, QuartColor);
                DebugOverlay.Line((face.topLeft - face.bottomLeft)*(3f/4f)+face.bottomLeft, (face.topRight - face.bottomRight)*(3f/4f)+face.bottomRight, QuartColor);

                List<Vector3> snapPoints = new();
                for(int x=0; x < 5; x++){
                    for(int y=0; y < 5; y++){
                        var xl = (face.topLeft - face.bottomLeft)*(y/4f)+face.bottomLeft;
                        var xr = (face.topRight - face.bottomRight)*(y/4f)+face.bottomRight;
                        snapPoints.Add((xr-xl)*(x/4f)+xl);
                    }
                }
                var nearest = snapPoints.OrderBy(x=>x.Distance(face.point)).First();
                if(Input.Down(InputButton.Use))
                    DebugOverlay.Sphere(nearest, 1f, Color.Red);
                return new TraceResult{
                    Bone = eyeTrace.Bone,
                    EndPosition = nearest,
                    Entity = m,
                    Hit = true,
                    Normal = m.Transform.NormalToWorld(face.normal),
                    StartPosition = eyeTrace.StartPosition,
		            StartedSolid = eyeTrace.StartedSolid,
		            Fraction = eyeTrace.Fraction,
		            Body = eyeTrace.Body,
		            Shape = eyeTrace.Shape,
		            HitboxIndex = eyeTrace.HitboxIndex,
		            Surface = eyeTrace.Surface,
		            Direction = eyeTrace.Direction
                };
            }
        }
        return eyeTrace;
    }

    public struct BoxFaceResult{
        public Vector3 point;
        public Vector3 bottomLeft;
        public Vector3 bottomRight;
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 normal;
        public BoxFaceResult(Vector3 point, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight, Vector3 normal){
            this.point = point;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.normal = normal;
        }
    }

    public static BoxFaceResult? PointOnBox(ModelEntity ent, Vector3 rayStart, Vector3 rayDir){
        var bb = ent.Model.Bounds;
        var bbScale = bb.Maxs - bb.Mins;
        var rStart = ent.Transform.PointToLocal(rayStart);
        var rDir = ent.Transform.NormalToLocal(rayDir);
        List<(Vector3 point, Vector3 normal)> pointsOnBox = new();
        foreach(var d in Direction.All){
            var dir = (d+1f)/2f;
            var planePos = dir*bbScale + bb.Mins;
            var planeRot = Rotation.LookAt(-d);
            var rayhit = LinePlaneIntersect(rStart, rDir, planePos, planeRot);
            var posOnPlane = (rayhit * planeRot) + planePos;
            bool OnBox = posOnPlane.WithinBB(bb, d);
            posOnPlane = ent.Transform.PointToWorld(posOnPlane);
            if(DEBUG){
                DebugOverlay.Sphere(posOnPlane, 2f, Color.Red,false,0);
                DebugOverlay.Line(ent.Transform.PointToWorld(planePos),ent.Transform.PointToWorld(planePos)+ent.Transform.NormalToWorld(planeRot.Forward)*2f, OnBox?Color.Green:Color.Red, 0, false);
            }
            if(OnBox)pointsOnBox.Add((posOnPlane, d));
        }
        if(!pointsOnBox.Any())return null;
        var point = pointsOnBox.OrderBy(x=>x.point.Distance(rayStart)).First();

        int axis = 0;
        var localNormal = point.normal;
        var localPoint = ent.Transform.PointToLocal(point.point);
        if(localNormal.y != 0)axis = 1;
        if(localNormal.z != 0)axis = 2;
        float left;
        float right;
        float top;
        float bottom;
        Vector3 tl;
        Vector3 bl;
        Vector3 tr;
        Vector3 br;
        switch(axis){
            case 0:
                left = bb.Mins.y;
                right = bb.Maxs.y;
                top = bb.Mins.z;
                bottom = bb.Maxs.z;
                tl = ent.Transform.PointToWorld(new Vector3(localPoint.x, left, top));
                bl = ent.Transform.PointToWorld(new Vector3(localPoint.x, left, bottom));
                tr = ent.Transform.PointToWorld(new Vector3(localPoint.x, right, top));
                br = ent.Transform.PointToWorld(new Vector3(localPoint.x, right, bottom));
                return new BoxFaceResult(point.point, bl, br, tl, tr, point.normal);
            case 1:
                left = bb.Mins.x;
                right = bb.Maxs.x;
                top = bb.Mins.z;
                bottom = bb.Maxs.z;
                tl = ent.Transform.PointToWorld(new Vector3(left, localPoint.y, top));
                bl = ent.Transform.PointToWorld(new Vector3(left, localPoint.y, bottom));
                tr = ent.Transform.PointToWorld(new Vector3(right, localPoint.y, top));
                br = ent.Transform.PointToWorld(new Vector3(right, localPoint.y, bottom));
                return new BoxFaceResult(point.point, bl, br, tl, tr, point.normal);
            case 2:
                left = bb.Mins.x;
                right = bb.Maxs.x;
                top = bb.Mins.y;
                bottom = bb.Maxs.y;
                tl = ent.Transform.PointToWorld(new Vector3(left, top, localPoint.z));
                bl = ent.Transform.PointToWorld(new Vector3(left, bottom, localPoint.z));
                tr = ent.Transform.PointToWorld(new Vector3(right, top, localPoint.z));
                br = ent.Transform.PointToWorld(new Vector3(right, bottom, localPoint.z));
                return new BoxFaceResult(point.point, bl, br, tl, tr, point.normal);
        }
        return null;
    }
    
    static class Direction {
        public static Vector3 Forward = new(1,0,0);
        public static Vector3 Left = new(0,1,0);
        public static Vector3 Up = new(0,0,1);
        public static Vector3 Back = new(-1,0,0);
        public static Vector3 Right = new(0,-1,0);
        public static Vector3 Down = new(0,0,-1);
        public static Vector3[] All => new[]{Forward,Left,Up,Back,Right,Down};
    }
}

public static class BBHasPoint {
    public static bool WithinBB(this Vector3 point, BBox box, Vector3 dir){
        return (point.x*(1f-Math.Abs(dir.x))) >= (box.Mins.x*(1f-Math.Abs(dir.x))-float.Epsilon) && (point.y*(1f-Math.Abs(dir.y))) >= (box.Mins.y*(1f-Math.Abs(dir.y))-float.Epsilon) && (point.z*(1f-Math.Abs(dir.z))) >= (box.Mins.z*(1f-Math.Abs(dir.z))-float.Epsilon)
            && (point.x*(1f-Math.Abs(dir.x))) <= (box.Maxs.x*(1f-Math.Abs(dir.x))+float.Epsilon) && (point.y*(1f-Math.Abs(dir.y))) <= (box.Maxs.y*(1f-Math.Abs(dir.y))+float.Epsilon) && (point.z*(1f-Math.Abs(dir.z))) <= (box.Maxs.z*(1f-Math.Abs(dir.z))+float.Epsilon);
    }
}