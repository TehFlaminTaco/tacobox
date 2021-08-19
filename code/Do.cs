using System;
using System.Collections.Generic;
using Sandbox;
using System.Linq;

public static class Do {
    public static List<(float time, Action act)> delayedActions = new();

    public static void After(float time, Action act){
        delayedActions.Add((Time.Now+time, act));
    }

    [Event.Tick]
    public static void HandleWaited(){

        foreach ((float time, Action act) in delayedActions.Where(x => x.time <= Time.Now)){
            act();
        }
        delayedActions.RemoveAll(x => x.time <= Time.Now);
    }
}