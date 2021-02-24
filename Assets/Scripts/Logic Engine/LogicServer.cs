using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using FullSerializer;
using System.Text;

public class LogicServer : MonoBehaviour
{
    public TextMeshProUGUI freqText;
    public static ulong ticks = 0;
    public CircuitDisplay mainCircuitDisplay;
    public TimeSpan computeBudget = TimeSpan.FromMilliseconds(2);

    private Queue<LogicEvent> eventQueue = new Queue<LogicEvent>();
    private Queue<Guid> gateQueue = new Queue<Guid>();

    private Circuit circuit = new Circuit("Main Circuit");
    Stopwatch precisionTimer = new Stopwatch();

    private static fsSerializer _serializer = new fsSerializer();
    

    // Start is called before the first frame update
    void Awake()
    {
        Circuit loadedCircuit = Load();

        if(loadedCircuit == null){
            circuit = new Circuit("Main Circuit");
        } else {
            circuit = loadedCircuit;
        }

        precisionTimer.Start();

        // Tell our main circuit display that its host circuit is
        // the main, top-level circuit.
        mainCircuitDisplay.circuitSimObject = circuit;

        StartCoroutine(RunSimulation());
    }

    void OnDestroy(){
        Save(circuit);
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

    public void SaveMain(){
        Save(circuit);
    }

    public void Save(Circuit c){

        // Serialize circuit state.
        fsSerializer _serializer = new fsSerializer();
        fsData data;
        try{
            _serializer.TrySerialize(c.GetType(), c, out data).AssertSuccessWithoutWarnings();
        } catch (Exception e){
            UnityEngine.Debug.LogError("Failed to serialize: " + e.ToString());
            return;
        }

        UnityEngine.Debug.Log(fsJsonPrinter.PrettyJson(data));
        byte[] compressedJson = Encoding.UTF8.GetBytes(fsJsonPrinter.CompressedJson(data));

        string destinationPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "save.json";

        FileStream saveFile;

        if(File.Exists(destinationPath)){
            File.Delete(destinationPath);
        }
        saveFile = File.Create(destinationPath);

        saveFile.Write(compressedJson, 0, compressedJson.Length);

        saveFile.Close();
    }

    public Circuit Load(){
        string destinationPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "save.json";

        if(File.Exists(destinationPath) == false){
            UnityEngine.Debug.Log("No save file found.");
            return null;
        }

        string saveData = File.ReadAllText(destinationPath, Encoding.UTF8);

        fsData parsedData = fsJsonParser.Parse(saveData);
        // step 2: deserialize the data
        object deserialized = null;
        _serializer.TryDeserialize(parsedData, typeof(Circuit), ref deserialized).AssertSuccessWithoutWarnings();

        return deserialized as Circuit;;
    }

}
