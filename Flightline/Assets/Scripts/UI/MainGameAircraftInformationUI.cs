using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MainGameAircraftInformationUI : MonoBehaviour
{
    public static MainGameAircraftInformationUI Instance { get; private set; }

    [SerializeField] TextMeshProUGUI throttleText;
    [SerializeField] TextMeshProUGUI airspeedText;
    [SerializeField] TextMeshProUGUI flapsText;
    [SerializeField] TextMeshProUGUI spoilersText;
    [SerializeField] TextMeshProUGUI altitudeText;
    [SerializeField] PhysicsCalculations plane;

    private float previousThrottle = 0;
    private int previousAirspeed = 0;
    private float previousAltitude = 0;

    private void Start()
    {
        Instance = this;
    }
    private void Update()
    {
        if (previousThrottle != (Mathf.Round(plane.Throttle * 100) / 100))
        {
            throttleText.text = "throttle: " + (Mathf.RoundToInt(plane.Throttle * 100)) + "%";
            previousThrottle = Mathf.Round(plane.Throttle * 100) / 100;
        }
        if (previousAirspeed != Mathf.RoundToInt(plane.Velocity.magnitude))
        {
            airspeedText.text = "airspeed: " + plane.Velocity.magnitude.ConvertTo<int>();
            previousAirspeed = plane.Velocity.magnitude.ConvertTo<int>();
        }
        if (previousAltitude != Mathf.RoundToInt(plane.transform.position.y))
        {
            altitudeText.text = "altitude: " + (Mathf.RoundToInt(plane.transform.position.y) * 10) + "ft";
            previousAltitude = plane.transform.position.y;
        }
    }

    public void UpdateFlapsText()
    {
        if (plane.FlapsDeployed)
        {
            flapsText.text = "flaps enabled";
        }
        else
        {
            flapsText.text = "flaps disabled";
        }
    }

    public void UpdateSpoilersText()
    {
        if (plane.AirbrakeDeployed)
        {
            Debug.Log("test2");
            spoilersText.text = "spoilers enabled";
        }
        else
        {
            Debug.Log("test3");
            spoilersText.text = "spoilers disabled";
        }
    }
}
