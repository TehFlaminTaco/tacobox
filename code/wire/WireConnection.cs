using System.Collections.Generic;
using Sandbox;
using System.Linq;

public partial class WireConnection {
    public Entity inEnt {get; set;}
    public string inID {get; set;}
    public Entity outEnt {get; set;}
    public string outID {get; set;}
    
    [ServerCmd]
    public static void MakeConnection(string buildString){
        var chunks = buildString.Split(':');
        Assert.True(chunks.Length == 4);

        MakeConnection(Entity.FindByIndex(chunks[0].ToInt()), chunks[1], Entity.FindByIndex(chunks[2].ToInt()), chunks[3]);
    }

    [ClientRpc]
    public static void MakeClientConnections(string buildString){
        var chunks = buildString.Split(':');
        Assert.True(chunks.Length == 4);

        MakeConnection(Entity.FindByIndex(chunks[0].ToInt()), chunks[1], Entity.FindByIndex(chunks[2].ToInt()), chunks[3]);
    }

    public static void MakeConnection(Entity inEnt, string inID, Entity outEnt, string outID){
        if(inEnt.IsServer)
            MakeClientConnections($"{inEnt.NetworkIdent}:{inID}:{outEnt.NetworkIdent}:{outID}");
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

    public static (Entity target, string id) GetConnection(Entity inEnt, string inID){
        var connection = allConnections.Where(x => x.inEnt == inEnt && x.inID == inID).FirstOrDefault();
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