using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

public abstract class Primitive : IComputable
{
    public Vector2 position {get; set;}
    public Guid guid {get; set;}
    public String name {get; set;}
    public LogicType logicType;
    public List<Net> inputs;
    public List<Net> outputs;

    public List<bool> stagedOutputs;

    public Circuit hostCircuit;

    public abstract void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue);
    
}

public class Not : Primitive
{
    public Not(string name){
        logicType = LogicType.NOT;
        inputs = new List<Net> { null };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        
        if(inputs[0] != null) {
            stagedOutputs[0] = !inputs[0].value;
        }
    }
}

public class And : Primitive
{
    public And(string name){
        logicType = LogicType.AND;
        inputs = new List<Net> { null, null };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        if(inputs[0] != null && inputs[1] != null) {
            stagedOutputs[0] = inputs[0].value && inputs[1].value;
        }
    }
}

public class Nand : Primitive
{
    public Nand(string name){
        logicType = LogicType.NAND;
        inputs = new List<Net> { null, null };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        if(inputs[0] != null && inputs[1] != null) {
            stagedOutputs[0] = !(inputs[0].value && inputs[1].value);
        }
    }
}

public class Or : Primitive
{
    public Or(string name){
        logicType = LogicType.AND;
        inputs = new List<Net> { null, null };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        if(inputs[0] != null && inputs[1] != null) {
            stagedOutputs[0] = inputs[0].value || inputs[1].value;
        }
    }
}

public class Nor : Primitive
{
    public Nor(string name){
        logicType = LogicType.AND;
        inputs = new List<Net> { null, null };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        if(inputs[0] != null && inputs[1] != null) {
            stagedOutputs[0] = !(inputs[0].value || inputs[1].value);
        }
    }
}

public class Xor : Primitive
{
    public Xor(string name){
        logicType = LogicType.AND;
        inputs = new List<Net> { null, null };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        if(inputs[0] != null && inputs[1] != null) {
            stagedOutputs[0] = inputs[0].value ^ inputs[1].value;
        }
    }
}

public class Xnor : Primitive
{
    public Xnor(string name){
        logicType = LogicType.AND;
        inputs = new List<Net> { null, null };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        if(inputs[0] != null && inputs[1] != null) {
            stagedOutputs[0] = !(inputs[0].value ^ inputs[1].value);
        }
    }
}

// Wrapper class for any primitives that generate signals.
// Used to preemptively enqueue net updates at sim init.
public abstract class Source : Primitive{

}

public class Lever : Source
{
    bool desiredValue;
    
    public Lever(string name){
        logicType = LogicType.Lever;
        inputs = new List<Net> { };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public void ToggleLever(){
        desiredValue = !desiredValue;
        Debug.Log("Lever desired value now " + desiredValue);
        if(outputs[0] != null){
            hostCircuit.NotifyGateChange(guid);
        }
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        
        stagedOutputs[0] = desiredValue;
        Debug.Log("Computing lever, staged output value = " + stagedOutputs[0]);
    }
}

public class Indicator : Primitive{
    public Indicator(string name){
        logicType = LogicType.Indicator;
        inputs = new List<Net> { null };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        stagedOutputs[0] = inputs[0]?.value ?? false;
        Debug.Log("Computed indicator, new status = " + stagedOutputs[0]);
    }
}
