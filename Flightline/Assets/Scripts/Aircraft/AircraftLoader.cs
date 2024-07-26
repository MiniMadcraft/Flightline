using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

public class AircraftLoader : MonoBehaviour
{
    public static AircraftLoader Instance { get; private set; }

    [SerializeField] private GameObject lightPlane;
    [SerializeField] private GameObject businessJet;
    [SerializeField] private GameObject eaglePlane;
    [SerializeField] private GameObject rafalePlane;

    public string aircraftSelection;
    async void Start()
    {
        Instance = this;
        Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "AircraftSelection" });
        aircraftSelection = savedData["AircraftSelection"].Value.GetAs<string>(); // Fetch the value held at this key

        switch (aircraftSelection)
        {
            case "Typhoon": // Check the given text against each possible aircraft
                lightPlane.SetActive(true);
                aircraftSelection = "Typhoon";
                break;
            case "Eagle": // Same as above
                eaglePlane.SetActive(true);
                aircraftSelection = "Eagle";
                break;
            case "Business Jet": // Same as above
                businessJet.SetActive(true);
                aircraftSelection = "Business Jet";
                break;
            case "Rafale": // Same as above
                rafalePlane.SetActive(true);
                aircraftSelection = "Rafale";
                break;
            default:
                break;
        }
    }
}
