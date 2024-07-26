using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] PhysicsCalculations plane;
    [SerializeField] GameInput gameInput;

    Vector3 controlInput;


    private void Start()
    {
        SetPlane(plane);
        gameInput.OnRollLeftAction += GameInput_OnRollLeftAction; // Subscribe to the events
        gameInput.OnRollRightAction += GameInput_OnRollRightAction;
        gameInput.OnThrottleUpAction += GameInput_OnThrottleUpAction;
        gameInput.OnThrottleDownAction += GameInput_OnThrottleDownAction;
        gameInput.OnSpoilersAction += GameInput_OnSpoilersAction;
        gameInput.OnPitchUpAction += GameInput_OnPitchUpAction;
        gameInput.OnPitchDownAction += GameInput_OnPitchDownAction;
        gameInput.OnFlapsDownAction += GameInput_OnFlapsDownAction;
        gameInput.OnFlapsUpAction += GameInput_OnFlapsUpAction;
    }

    private void SetPlane(PhysicsCalculations plane)
    {
        this.plane = plane;
    }

    public void GameInput_OnThrottleUpAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        if (!GameInput.Instance.playerInputActions.Plane.throttleUp.enabled) return;
        if (plane.Throttle != 1f)
        {
            plane.SetThrottleInput(plane.Throttle + 0.02f); // Increment the throttle input
        }
    }

    public void GameInput_OnThrottleDownAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        if (!GameInput.Instance.playerInputActions.Plane.throttleDown.enabled) return;
        if (plane.Throttle != 0f)
        {
            plane.SetThrottleInput(plane.Throttle - 0.02f); // Decrement the throttle input
        }
    }
    public void GameInput_OnRollLeftAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        if (!GameInput.Instance.playerInputActions.Plane.rollLeft.enabled) return;
        controlInput = new Vector3(controlInput.x, controlInput.y, 1); // Roll the aircraft to the left
    }
    public void GameInput_OnRollRightAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        if (!GameInput.Instance.playerInputActions.Plane.rollRight.enabled) return;
        controlInput = new Vector3(controlInput.x, controlInput.y, -1); // Roll the aircraft to the right
    }
    public void GameInput_OnSpoilersAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        plane.ToggleSpoilers();
    }
    public void GameInput_OnPitchUpAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        if (!GameInput.Instance.playerInputActions.Plane.pitchUp.enabled) return;
        controlInput = new Vector3(-1, controlInput.y, controlInput.z); // Pitch up the aircraft
    }
    public void GameInput_OnPitchDownAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        if (!GameInput.Instance.playerInputActions.Plane.pitchDown.enabled) return;
        controlInput = new Vector3(1, controlInput.y, controlInput.z); // Pitch down the aircraft
    }
    public void GameInput_OnFlapsDownAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        if (!GameInput.Instance.playerInputActions.Plane.flapsDown.enabled) return;
        if (plane.FlapsDeployed) return;
        plane.ToggleFlaps(); // Toggle the flaps
    }
    public void GameInput_OnFlapsUpAction(object sender, EventArgs e) // On the event being called...
    {
        if (plane == null) return;
        if (!GameInput.Instance.playerInputActions.Plane.flapsUp.enabled) return;
        if (!plane.FlapsDeployed) return;
        plane.ToggleFlaps(); // Toggle the flaps
    }

    private void Update()
    {
        if (plane == null) return;
        plane.SetControlInput(controlInput);
        controlInput = new Vector3(0, 0, 0);
    }
}
