using System.Collections.Generic;
using Sandbox;
using System.Linq;

public partial class WireConnection {
    public Entity inEnt {get; set;}
    public string inID {get; set;}
    public Entity outEnt {get; set;}
    public string outID {get; set;}
    
    public static void MakeConnection(Entity inEnt, string inID, Entity outEnt, string outID){
        allConnections.RemoveAll(x => x.inEnt == inEnt && x.inID == inID);
        if(outEnt is null || outID is null)
            return;
        
        WireConnection connect = new();
        connect.inEnt = inEnt;
        connect.inID = inID;
        connect.outEnt = outEnt;
        connect.outID = outID;
        allConnections.Add(connect);
    }

    public static void BreakConnection(Entity inEnt, string inID){
        allConnections.RemoveAll(x => x.inEnt == inEnt && x.inID == inID);
    }

    public (Entity target, string id) GetConnection(Entity inEnt, string inID){
        var connection = allConnections.Where(x => x.inEnt == inEnt && x.inID == inID).First();
        if(connection == null){
            return (null, null);
        }else{
            return (connection.outEnt, connection.outID);
        }
    }

    [Event.Tick]
    public static void SimulateConnections(){
        allConnections.RemoveAll(x =>!x.inEnt.IsValid() || !x.outEnt.IsValid());
        foreach(var connection in allConnections){
            WireVal.FromID(connection.inEnt, connection.inID)?.CopyFrom(
                WireVal.FromID(connection.outEnt, connection.outID)
            );
        }
    }

    public static List<WireConnection> allConnections = new();
}