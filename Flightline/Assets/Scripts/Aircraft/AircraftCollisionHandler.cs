using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftCollisionHandler : MonoBehaviour
{
    [SerializeField] PhysicsCalculations plane;
    [SerializeField] GameObject runway;

    public AnimationCurve landingVelocityCurve;
    private float time;
    private float landingScore;
    private float flightScore;
    private void Update()
    {
        time += Time.deltaTime;
        if (time > 3f && landingScore != 0f && flightScore != 0f) // If more than 3 seconds (load) and landing score isnt 0 and flight score isnt 0
        {
            if (Mathf.RoundToInt(plane.Velocity.sqrMagnitude) == 0) // If velocity is 0
            {
                FlightCompletionUI.Instance.SetFadeIn(Mathf.RoundToInt(landingScore), Mathf.RoundToInt(flightScore * 0.1f)); // Fade in flight completion ui passing through scores
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        float yVelocity = plane.Velocity.y < 0 ? plane.Velocity.y * -1f: plane.Velocity.y; // Fetch y velocity as positive val
        if (collision.gameObject.name == "Cube" && landingVelocityCurve.Evaluate(yVelocity) > 5f) // If collided with runway and score greater than 5
        {
            if (time > 3f) // If time greater than 3 (load)
            {
                landingScore = landingVelocityCurve.Evaluate(yVelocity);
                flightScore = time; // Set landing and flight scores
            }
            else
            {
                landingScore = 0f; // Crashed
                flightScore = 0f;
            }
        }
        else
        {
            FlightCompletionUI.Instance.SetFadeIn(0, 0); // Crashed
        }
    }
}
