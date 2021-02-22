using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Circuit : Primitive
{

    public Vector2 windowSize = new Vector2(500,500);
    public Dictionary<Guid, Net> nets = new Dictionary<Guid, Net>();
    public Dictionary<Guid, Primitive> gates = new Dictionary<Guid, Primitive>();

    // Internal compute queues
    Queue<LogicEvent> eventQueue = new Queue<LogicEvent>();
    HashSet<LogicEvent> eventQueueHashes = new HashSet<LogicEvent>();
    Queue<Guid> gateQueue = new Queue<Guid>();

    // hashset for quickly checking presence of gates in gatequeue
    HashSet<Guid> gateQueueHashes = new HashSet<Guid>();

    List<ITickable> tickables = new List<ITickable>();

    public Circuit(string name){
        inputs = new List<Net>();
        outputs = new List<Net>();
        this.name = name;
        guid = Guid.NewGuid();
    }

    public void Add(Primitive primitive){
        Add(primitive.guid, primitive);
        // localComputeQueue.Enqueue(computable);
    }

    public void Add(Guid newGuid, Primitive primitive){
        gates.Add(newGuid, primitive);

        // Tell the primitive that we're its host circuit
        // Necessary so that user-input primitives can
        // trigger logic events
        primitive.hostCircuit = this;

        ITickable tickable = primitive as ITickable;

        if(tickable != null){
            tickables.Add(tickable);
        }
    }

    public void NotifyGateChange(Guid gateGuid){
        gateQueue.Enqueue(gateGuid);
        gateQueueHashes.Add(gateGuid);
    }

    // Returns net GUID
    public Guid Connect(Guid src, int srcIndex, Guid dest, int destIndex){
        return Connect(gates[src], srcIndex, gates[dest], destIndex);
    }

    // Returns net GUID
    public Guid Connect(Primitive src, int srcIndex, Primitive dest, int destIndex){
        Debug.Log($"Connecting {src.name}[{srcIndex}] to {dest.name}[{destIndex}]");

        // For now, don't overwrite net connections. Could leak orphaned nets :(
        Debug.Assert(dest.inputs[destIndex] == null);

        Guid netGuid;

        if(src.outputs[srcIndex] != null){
            Debug.Log("Adding dest to existing fanout");
            src.outputs[srcIndex].fanout.Add(dest);
            dest.inputs[destIndex] = src.outputs[srcIndex];

            // Return new net guid
            netGuid = src.outputs[srcIndex].guid;
        } else {
            Net newNet = new Net();
            nets.Add(newNet.guid, newNet);
            
            // Connect the primitives
            src.outputs[srcIndex] = newNet;
            dest.inputs[destIndex] = newNet;
            newNet.fanout.Add(dest);
            newNet.source = src;

            // Return new net guid
            netGuid = newNet.guid;
        }

        // Check to see if net needs to be queued at sim init
        if(src is Source){
            eventQueue.Enqueue(new LogicEvent() {
                netGuid = src.outputs[srcIndex].guid,
                newValue = src.outputs[srcIndex].value
            });
        }

        return netGuid;
    }


    // Removes a primitive and updates relevant nets. Returns a list of
    // all nets that were touched.
    public List<Guid> RemovePrimitive(Guid guid){
        Primitive p = gates[guid];

        List<Guid> netsToUpdate = new List<Guid>();
        List<Guid> netsToDelete = new List<Guid>();
        
        // Update inbound nets to no longer have this primitive as a fanout
        foreach(var inboundNet in p.inputs){
            if(inboundNet != null){
                inboundNet.fanout.Remove(p);
                Debug.Log("Adding net to update, source = " + inboundNet.source);
                netsToUpdate.Add(inboundNet.guid);
                if(inboundNet.fanout.Count == 0){
                    Debug.Log("Adding net to delete");
                    netsToDelete.Add(inboundNet.guid);  
                }
            }
        }

        // Delete outbound nets.
        foreach(var outboundNet in p.outputs){
            // If port disconnected, keep going
            if(outboundNet == null){ continue; }
            foreach(var recipient in outboundNet.fanout){
                // Sever the connection
                recipient.inputs[recipient.inputs.IndexOf(outboundNet)] = null;
            }
            netsToDelete.Add(outboundNet.guid);
            netsToUpdate.Add(outboundNet.guid);
        }

        // Delete any nets that need deleting.
        foreach(Guid n in netsToDelete){
            nets[n].source.outputs[nets[n].source.outputs.IndexOf(nets[n])] = null;
            nets.Remove(n);
        }

        if((gates[guid] as ITickable) != null){
            tickables.Remove(gates[guid] as ITickable);
        }

        gates.Remove(guid);



        return netsToUpdate;
    }

    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        
        // Debug.Log("Event queue count: " + eventQueue.Count);

        // Flush event queue
        while(eventQueue.Count != 0){
            LogicEvent e = eventQueue.Dequeue();
            //Debug.Log("Dequeuing event");
            // Apply new net value from the logic event queue
            nets[e.netGuid].value = e.newValue;

            foreach(var gate in nets[e.netGuid].fanout){

                // Add each gate in fanout to the gate queue,
                // unless the gate is already present
                if(!gateQueueHashes.Contains(gate.guid)){
                    gateQueue.Enqueue(gate.guid);
                }
            }
        }

        // Flush gate queue
        while(gateQueue.Count != 0){
            
            var gateGuid = gateQueue.Dequeue();
            gateQueueHashes.Remove(gateGuid);
            var gate = gates[gateGuid];
            //Debug.Log("Popping gate from queue, name = " + gate.name);

            List<bool> prevOutput = new List<bool>();
            foreach(var output in gate.outputs){

                // Pin on primitive not connected.
                if(output == null){
                    prevOutput.Add(false);
                } else {
                    prevOutput.Add(output.value);
                }
            }

            // Compute primitive
            //Debug.Log("Calling Compute() on gate");
            gate.Compute(parentGateQueue, parentEventQueue);

            // Compare new output
            for(int i = 0; i < gate.outputs.Count; i++){

                // Output not connected
                if(gate.outputs[i] == null) { continue; }

                bool newOutput = gate.stagedOutputs[i];
                //Debug.Log($"New output {newOutput}, prev output {prevOutput[i]}");
                // If value has changed, or if the net is init-flagged
                if(newOutput != prevOutput[i] || gate.outputs[i].initFlag) {
                    
                    if(gate.outputs[i].initFlag){
                        //Debug.Log("Flagged net, queueing event");
                    } else {
                        //Debug.Log("Net change detected, queueing event");
                    }
                    
                    // Construct a new logic event, indicating
                    // the guid of the connected net and the new value
                    LogicEvent newEvent = new LogicEvent() {
                        netGuid = gate.outputs[i].guid,
                        newValue = newOutput
                    };

                    // Clear init flag
                    gate.outputs[i].initFlag = false;

                    eventQueue.Enqueue(newEvent);
                }

            }
        }

        foreach(var tickable in tickables){
            tickable.Tick(eventQueue);
        }
        

        // if(localComputeQueue.Count == 0){
        //     var somethingToEnqueue = foreach(var pair in circuit)
        //     localComputeQueue.Enqueue(circuit.Values.);
        // }

        // // We compute our local compute queue
        // IComputable thing = localComputeQueue.Dequeue();
        // thing.Compute(ref localComputeQueue);

        // // Then simply queue ourselves in the parent queue again,
        // // so that we are called again next tick.
        // parentQueue.Enqueue(this);
    }

}
