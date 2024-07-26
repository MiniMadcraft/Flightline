using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using System.Linq;

public class FlightDataHandler : MonoBehaviour
{
    public static FlightDataHandler Instance { get; private set; }

    public FlightData flightData;
    async void Start()
    {
        Instance = this;
        byte[] json = await CloudSaveService.Instance.Files.Player.LoadBytesAsync("FlightData"); // Fetch the flight data file
        string jsonString = System.Text.Encoding.UTF8.GetString(json);
        Debug.Log(jsonString);
        flightData = JsonUtility.FromJson<FlightData>(jsonString); // Convert the fetched data to the FlightData type
    }

    public async Task SaveFlightData(FlightEntry flightEntry)
    {
        FlightEntry newFlightEntry = new FlightEntry(flightEntry.date, flightEntry.aircraft, flightEntry.score); // Ensures it doesn't use a previous instance
        flightData.results.Add(newFlightEntry); // Add the entry to the end of the list
        Debug.Log(JsonConvert.SerializeObject(flightData.results));
        string dataPath = Application.persistentDataPath + "/playerData.json"; // Temporarily store at the persistent data path
        string data = JsonConvert.SerializeObject(flightData); // Convert to JSON string
        File.WriteAllText(dataPath, data); // Write to persistent data path
        byte[] file = File.ReadAllBytes(dataPath); // Read from persistent data path
        await CloudSaveService.Instance.Files.Player.SaveAsync("FlightData", file); // Save to users Cloud Save "FlightData" file
        Debug.Log(data);
    }
}

[System.Serializable]
public class FlightEntry
{
    public string date;
    public string aircraft;
    public int score;

    public FlightEntry(string _date, string _aircraft, int _score) // Holds all fields in a flight entry
    {
        date = _date;
        aircraft = _aircraft;
        score = _score;
    }
}

[System.Serializable]
public class FlightData
{
    public List<FlightEntry> results; // Holds the list containing all individual flight entries
}