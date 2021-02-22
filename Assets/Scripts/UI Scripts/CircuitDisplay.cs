using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

public class CircuitDisplay : CircuitDevice
{
    public GameObject gateContainer;
    public GameObject wireContainer;
    public GameObject wirePrefab;

    public TextMeshProUGUI circuitLabel;
    public RectTransform contentRoot;
    public GameObject notGate;
    public GameObject andGate;
    public GameObject nandGate;
    public GameObject orGate;
    public GameObject norGate;
    public GameObject xorGate;
    public GameObject xnorGate;
    public GameObject leverGate;
    public GameObject indicatorGate;
    public GameObject clockGate;
    public GameObject shiftRegister4Gate;
    public GameObject shiftRegister8Gate;
    public GameObject subcircuitPrefab;
    public Circuit circuitSimObject;

    private Dictionary<Guid, CircuitDevice> physicalDevices = new Dictionary<Guid, CircuitDevice>();
    private Dictionary<Guid, WireSystem> wires = new Dictionary<Guid, WireSystem>();

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        logicType = Utils.LogicType.Circuit;
        PopulateCircuit();
    }

    void PopulateCircuit(){

        circuitLabel.text = circuitSimObject.name;

        foreach(var simGate in circuitSimObject.gates){
            PopulateSimGate(simGate.Value);
        }

        // Spawn all wire systems to display connections.
        foreach(var device in physicalDevices){

            // Lookup the internal logic sim object
            var simObject = circuitSimObject.gates[device.Key] as Primitive;

            // If the sim object is not a primitive, we don't need
            // to spawn any wire connections.
            if(simObject == null) {
                continue;
            }

            for(int connectionIndex = 0; connectionIndex < simObject.outputs.Count; connectionIndex++){
                if(simObject.outputs[connectionIndex] != null){
                    PopulateWire(simObject.outputs[connectionIndex].guid);
                }
            }
        }
    }

    void PopulateWire(Guid netGuid){

        // If we already have a wire system for this particular net...
        if(wires.ContainsKey(netGuid)){
            Debug.Log("Destroying old wire system");    
            Destroy(wires[netGuid].gameObject);
            wires.Remove(netGuid);
        }

        Net net = circuitSimObject.nets.ContainsKey(netGuid) ? circuitSimObject.nets[netGuid] : null;

        if(net == null){
            Debug.Log("Net does not exist. Removed wire, and returning");
            return;
        }
        

        // // If the circuit sim does not contain the source of the net,
        // // it was deleted.
        // if(circuitSimObject.gates.ContainsKey(src.guid) == false){
        //     Debug.Log("Detected destroyed gate, returning");
        //     return;
        // }

        // // Don't create a wire system if the circuit sim doesn't contain the net...
        // if(circuitSimObject.nets.ContainsKey(netGuid) == false){
        //     Debug.Log("Detected destroyed net, returning");
        //     return;
        // }

        WireSystem newWire = GameObject.Instantiate(wirePrefab, wireContainer.transform).GetComponent<WireSystem>();
        newWire.transform.position = Vector2.zero;

        // Physically connect the wire system to the physical
        // connection port on the source device
        newWire.start = physicalDevices[net.source.guid].outputs[net.source.outputs.IndexOf(net)];

        // Find all receiving devices and their connection points
        foreach(Primitive recipient in net.fanout){
            
            // Null if point is not connected
            // if(recipient == null){ continue; }
            
            // Find which port this net is connected to
            int recipientPortIndex = recipient.inputs.IndexOf(net);

            CircuitDevice recipientDevice = physicalDevices[recipient.guid];
            ConnectionPoint recipientConnectionPoint = recipientDevice.inputs[recipientPortIndex];

            newWire.ends.Add(recipientConnectionPoint);
        }

        newWire.netGuid = netGuid;
        wires.Add(newWire.netGuid, newWire);
    }

    void PopulateSimGate(Primitive simGate){

        GameObject prefabToUse;

        switch(simGate.logicType){
            case Utils.LogicType.NOT:
                prefabToUse = notGate;
                break;
            case Utils.LogicType.AND:
                prefabToUse = andGate;
                break;
            case Utils.LogicType.NAND:
                prefabToUse = nandGate;
                break;
            case Utils.LogicType.OR:
                prefabToUse = orGate;
                break;
            case Utils.LogicType.NOR:
                prefabToUse = norGate;
                break;
            case Utils.LogicType.XOR:
                prefabToUse = xorGate;
                break;
            case Utils.LogicType.XNOR:
                prefabToUse = xnorGate;
                break;
            case Utils.LogicType.Lever:
                prefabToUse = leverGate;
                break;
            case Utils.LogicType.Indicator:
                prefabToUse = indicatorGate;
                break;
            case Utils.LogicType.Clock:
                prefabToUse = clockGate;
                break;
            case Utils.LogicType.ShiftRegister4:
                prefabToUse = shiftRegister4Gate;
                break;
            case Utils.LogicType.ShiftRegister8:
                prefabToUse = shiftRegister8Gate;
                break;
            default:
                prefabToUse = null;
                break;
        }

        if(prefabToUse == null){
            Debug.Log("Unknown simGate: " + simGate.GetType());
            return;
        }

        CircuitDevice gateObject = GameObject.Instantiate(prefabToUse, gateContainer.transform).GetComponent<CircuitDevice>();
        gateObject.transform.localPosition = (Vector3)simGate.position;
        gateObject.transform.SetAsFirstSibling();
        gateObject.associatedGuid = simGate.guid;
        gateObject.logicType = simGate.logicType;

        // If the new gate is a lever, tell it what the sim gate is,
        // so it can inform the logic sim of any new inputs.
        if(simGate.logicType == Utils.LogicType.Lever){
            (gateObject as LeverDevice).leverGate = simGate as Lever;
        }
        
        physicalDevices.Add(simGate.guid, gateObject);
    }

    public void AddPhysicalGate(CircuitDevice circuitDevice){
        Primitive newSimGate;

        switch(circuitDevice.logicType){
            case Utils.LogicType.NOT:
                newSimGate = new Not("replaceMe");
                break;
            case Utils.LogicType.NAND:
                newSimGate = new Nand("replaceMe");
                break;
            case Utils.LogicType.AND:
                newSimGate = new And("replaceMe");
                break;
            case Utils.LogicType.OR:
                newSimGate = new Or("replaceMe");
                break;
            case Utils.LogicType.NOR:
                newSimGate = new Nor("replaceMe");
                break;
            case Utils.LogicType.XOR:
                newSimGate = new Xor("replaceMe");
                break;
            case Utils.LogicType.XNOR:
                newSimGate = new Xnor("replaceMe");
                break;
            case Utils.LogicType.Lever:
                newSimGate = new Lever("replaceMe");
                (circuitDevice as LeverDevice).leverGate = newSimGate as Lever;
                break;
            case Utils.LogicType.Indicator:
                newSimGate = new Indicator("replaceMe");
                break;
            case Utils.LogicType.Clock:
                newSimGate = new Clock("replaceMe");
                break;
            case Utils.LogicType.ShiftRegister4:
                newSimGate = new ShiftRegister("replaceMe", 4);
                break;
            case Utils.LogicType.ShiftRegister8:
                newSimGate = new ShiftRegister("replaceMe", 8);
                break;
            default:
                Debug.LogError("Unknown circuit device type, object name: " + circuitDevice.gameObject.name);
                return;
        }

        newSimGate.position = circuitDevice.transform.localPosition;
        circuitDevice.associatedGuid = newSimGate.guid;

        circuitSimObject.Add(newSimGate);

        physicalDevices.Add(newSimGate.guid, circuitDevice);
    }

    public void RemovePhysicalGate(CircuitDevice circuitDevice){
        
        List<Guid> netsToUpdate = circuitSimObject.RemovePrimitive(circuitDevice.associatedGuid);
        Debug.Log($"{netsToUpdate.Count} nets need updating");
        foreach(Guid netGuid in netsToUpdate){
            PopulateWire(netGuid);
        }
        physicalDevices.Remove(circuitDevice.associatedGuid);
        Destroy(circuitDevice.gameObject);
    }

    // Returns true if a connection is possible.
    public bool CanMakeConnection(ConnectionPoint src, ConnectionPoint dest){

        if(src == null || dest == null){
            return false;
        }

        if(src.connectionType == ConnectionPoint.ConnectionType.Input || dest.connectionType == ConnectionPoint.ConnectionType.Output){
            return false;
        }

        if(src.connectionType == dest.connectionType) {
            return false;
        }

        CircuitDevice dest_cd = dest.GetComponentInParent<CircuitDevice>();

        // If we're trying to connect to a port that already has something connected to it, fail
        if(circuitSimObject.gates[dest_cd.associatedGuid].inputs[dest.connectionIndex] != null){
            return false;
        }

        return true;
    }

    // Create a connection between two physical connection points.
    public void MakeConnection(ConnectionPoint src, ConnectionPoint dest){
        Debug.Log("Submitting new connection to server: " + src.GetComponentInParent<CircuitDevice>().associatedGuid + ", " + dest.GetComponentInParent<CircuitDevice>().associatedGuid);
        Guid netGuid = circuitSimObject.Connect(src.GetComponentInParent<CircuitDevice>().associatedGuid, src.connectionIndex,
                                dest.GetComponentInParent<CircuitDevice>().associatedGuid, dest.connectionIndex);
        PopulateWire(netGuid);
    }

    void UpdateWireValues(){

        // TODO: Replace with selective (viewport-local) update
        foreach(var wire in wires){

            // Lookup the internal logic sim net
            Net net = circuitSimObject.nets[wire.Value.netGuid];

            wire.Value.SetValue(net.value);
        }

        // TODO: Replace with selective (viewport-local) update
        foreach(var device in physicalDevices){
            var stagedOutputs = circuitSimObject.gates[device.Key].stagedOutputs;
            for(int i = 0; i < stagedOutputs.Count; i++){
                device.Value.outputs[i].SetValue(stagedOutputs[i]);
            }
            (device.Value as IndicatorDevice)?.SetStatus(circuitSimObject.gates[device.Key].stagedOutputs[0]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWireValues();
        if(Input.GetKeyDown(KeyCode.Delete)){
            RemovePhysicalGate(EventSystem.current.currentSelectedGameObject.GetComponent<CircuitDevice>());
        }
    }
}
