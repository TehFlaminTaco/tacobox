using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public abstract class Gate {
    public string Key;
    public string Name;
    public string Description;
    public (WireVal.Type t, string key, string name)[] Inputs;
    public (WireVal.Type t, string key, string name)[] Outputs;

    public abstract void Process(GateEntity ent);
    
    public static Dictionary<string, List<Gate>> knownGates;
    public static Dictionary<string, Gate> gatesByKey;
    public static void RegisterGate(string category, Gate gate){
        if(knownGates is null)
            knownGates = new();
        if(!knownGates.ContainsKey(category))
            knownGates[category] = new();
        if(gatesByKey is null)
            gatesByKey = new();
        if(gatesByKey.ContainsKey(gate.Key))
            throw new Exception($"Gate {gate.Key} already registered!");

        knownGates[category].Add(gate);

        gatesByKey[gate.Key] = gate;
    }

    public static Gate getFromKey(string key){
        if(gatesByKey is null)
            return null;
        return gatesByKey.ContainsKey(key) ? gatesByKey[key] : null;
    }

    
    [Event.Hotload]
    public static void ResetGates(){
        knownGates = new();
        gatesByKey = new();

        RegisterGate("Maths", new GateMathsAdd());
        RegisterGate("Maths", new GateMathsSubtract());
        RegisterGate("Maths", new GateMathsMultiply());
        RegisterGate("Maths", new GateMathsDivide());

        RegisterGate("Vector", new GateVectorGPS()); 
        RegisterGate("Vector", new GateVectorAdd()); 
        RegisterGate("Vector", new GateVectorSubtract()); 
        RegisterGate("Vector", new GateVectorMultiply()); 
        RegisterGate("Vector", new GateVectorMagnitude()); 
        RegisterGate("Vector", new GateVectorMake()); 
        RegisterGate("Vector", new GateVectorBreak());

        RegisterGate("Rotation", new GateRotationForward());
        RegisterGate("Rotation", new GateRotationRight());
        RegisterGate("Rotation", new GateRotationUp());
        RegisterGate("Rotation", new GateRotationAroundAxis());
        RegisterGate("Rotation", new GateRotationMake());
        RegisterGate("Rotation", new GateRotationBreak());

        RegisterGate("Misc", new GateConstantValue());
    }

    public virtual void GenerateControls(Form inspector){
        
    }

    public virtual void DestroyControls(){

    }

    public virtual void TickControls(){

    }

    public virtual List<WireVal> GenerateValues(Player ply, GateEntity ent){
        return null;
    }

}