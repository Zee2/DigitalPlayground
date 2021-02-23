using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Net
{
    public Guid guid{get; set;}
    public List<Primitive> fanout = new List<Primitive>();
    public Primitive source;
    public bool value;
    public bool initFlag;

    public Net(){
        guid = Guid.NewGuid();
        // All new nets are flagged for initialization
        initFlag = true;
    }

    // public void OnBeforeSerialize(){
    //     guidString = guid.ToString();
    //     sourceGuidString = (source?.guid ?? Guid.Empty).ToString() ;
    //     fanoutGuids.Clear();
    //     foreach(Primitive p in fanout){
    //         fanoutGuids.Add(p.guid.ToString());
    //     }
    // }

    // public void OnAfterDeserialize(){
    //     guid = Guid.Parse(guidString);
    //     // Call Hydrate() later, when primitive references
    //     // have been populated.
    // }
    
    // // Populate fanout and source with references to the relevant primitives
    // public void Hydrate(Circuit hostCircuit){
    //     fanout.Clear();
    //     foreach(var fanoutGuid in fanoutGuids){
    //         Guid g = Guid.Parse(fanoutGuid);
    //         fanout.Add(g == Guid.Empty ? null : hostCircuit.gates[g]);
    //     }

    //     Guid sourceGuid = Guid.Parse(sourceGuidString);
    //     source = sourceGuid == Guid.Empty ? null : hostCircuit.gates[sourceGuid];
    // }

    // public void Compute(ref Queue<IComputable> computeQueue){
        
    //     // Propagate
    //     // If we're a terminal node (i.e. an input to a primitive),
    //     // simply queue our host primitive.
    //     if(host != null){
    //         //Debug.Log("Terminal node, enqueueing host");
    //         computeQueue.Enqueue(host);
    //     } else {

    //         //Debug.Log("Non-terminal node, enqueueing children");
    //         // Otherwise, we must enqueue all children to be computed
    //         // in the next tick cycle.
    //         foreach(var child in children){

    //             // Propagate value
    //             child.value = value;

    //             // We perform a one-in-advance check to see if the node is terminal.
    //             // This will allow directly connected primitives to compute immediately.
    //             if(child.host != null){
    //                 computeQueue.Enqueue(child.host);
    //             } else {
    //                 // Otherwise, we let the child node compute next cycle.
    //                 computeQueue.Enqueue(child);
    //             }
    //         }
    //     }
    // }
}
