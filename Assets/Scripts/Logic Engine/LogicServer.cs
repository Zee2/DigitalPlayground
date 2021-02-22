using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;

public class LogicServer : MonoBehaviour
{
    public TextMeshProUGUI freqText;
    public static ulong ticks = 0;
    public CircuitDisplay mainCircuitDisplay;
    public TimeSpan computeBudget = TimeSpan.FromMilliseconds(1);

    public float simFrequency = 10.0f;

    private Queue<LogicEvent> eventQueue = new Queue<LogicEvent>();
    private Queue<Guid> gateQueue = new Queue<Guid>();

    private Circuit circuit = new Circuit("Main Circuit");
    Stopwatch precisionTimer = new Stopwatch();
    

    // Start is called before the first frame update
    void Awake()
    {
        // Populate our circuit with primitives
        Lever lever1 = new Lever("lever1");
        lever1.position = new Vector2(100, -200);
        circuit.Add(lever1);

        Lever lever2 = new Lever("lever2");
        lever2.position = new Vector2(100, -400);
        circuit.Add(lever2);

        Nand nand1 = new Nand("nand1");
        nand1.position = new Vector2(300, -200);
        circuit.Add(nand1);

        Nand nand2 = new Nand("nand2");
        nand2.position = new Vector2(300, -400);
        circuit.Add(nand2);

        //circuit.Connect(lever1, 0, nand1, 0);
        //circuit.Connect(lever2, 0, nand2, 1);

        circuit.Connect(nand1, 0, nand2, 0);
        circuit.Connect(nand2, 0, nand1, 1);

        Indicator ind1 = new Indicator("ind1");
        ind1.position = new Vector2(500, -200);
        circuit.Add(ind1);
        circuit.Connect(nand1, 0, ind1, 0);

        Indicator ind2 = new Indicator("ind2");
        ind2.position = new Vector2(500, -400);
        circuit.Add(ind2);
        circuit.Connect(nand2, 0, ind2, 0);

        precisionTimer.Start();

        // Tell our main circuit display that its host circuit is
        // the main, top-level circuit.
        mainCircuitDisplay.circuitSimObject = circuit;

        StartCoroutine(RunSimulation());
    }

    IEnumerator RunSimulation(){
        
        while(true){
            var startTime = precisionTimer.Elapsed;
            while(precisionTimer.Elapsed - startTime < computeBudget){
                circuit.Compute(gateQueue, eventQueue);
                ticks++;
            }
            // UnityEngine.Debug.Log((ticks / precisionTimer.Elapsed.TotalSeconds) + " ticks/s");
            freqText.text = "Simulation Frequency: " + ((ticks / precisionTimer.Elapsed.TotalSeconds) / 1000.0).ToString("F0") + " KHz";
            yield return null;
        }
    }

    // Update is called once per frame
    // void Update()
    // {
        
            

    //     // UnityEngine.Debug.Log("notGate output: " + notGate.outputs[0].value);
    //     // UnityEngine.Debug.Log("anotherGate output: " + anotherGate.outputs[0].value);
    //     // UnityEngine.Debug.Log("yetAnotherGate output: " + yetAnotherGate.outputs[0].value);
    // }
}
