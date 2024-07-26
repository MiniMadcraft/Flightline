using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave.Internal.Http;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class PhysicsCalculations : MonoBehaviour
{
    public static PhysicsCalculations Instance {  get; private set; }

    [SerializeField] float maxThrust;
    [SerializeField] float throttleSpeed;
    [SerializeField] float gForceLimit;
    [SerializeField] float gForceLimitPitch;

    [Header("Lift")]
    [SerializeField] AnimationCurve liftAngleOfAttackCurve;
    [SerializeField] float inducedDrag;
    [SerializeField] AnimationCurve inducedDragCurve;
    [SerializeField] float rudderPower;
    [SerializeField] AnimationCurve rudderAngleOfAttackCurve;
    [SerializeField] AnimationCurve rudderInducedDragCurve;
    [SerializeField] float flapsLiftPower;
    [SerializeField] float flapsAngleOfAttackBias;
    [SerializeField] float flapsDrag;
    [SerializeField] float flapsRetractSpeed;

    [Header("Turning Aircraft")]
    [SerializeField] Vector3 turnSpeed;
    [SerializeField] Vector3 turnAcceleration;
    [SerializeField] AnimationCurve steeringCurve;

    [Header("Drag")]
    [SerializeField] AnimationCurve dragForward;
    [SerializeField] AnimationCurve dragBack;
    [SerializeField] AnimationCurve dragLeft;
    [SerializeField] AnimationCurve dragRight;
    [SerializeField] AnimationCurve dragTop;
    [SerializeField] AnimationCurve dragBottom;
    [SerializeField] Vector3 angularDrag;
    [SerializeField] float airbrakeDrag;

    [Header("Miscellaneous")]
    [SerializeField] bool flapsDeployed;
    [SerializeField] float initialSpeed;

    float throttleInput = 0.01f;
    float previousThrottleInput;
    Vector3 controlInput;

    Vector3 lastVelocity;

    public float Throttle { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Vector3 EffectiveInput { get; private set; }
    public Vector3 LocalVelocity { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 LocalGForce { get; private set; }
    public Vector3 LocalAngularVelocity { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float AngleOfAttackYaw { get; private set; }
    public bool AirbrakeDeployed = false;

    public bool FlapsDeployed
    {
        get
        {
                return flapsDeployed;
        }
        private set
        {
            flapsDeployed = value;

            //foreach (var lg in landingGear)
            //{
                //lg.enabled = value;
            //}
        }
    }

    private int liftPower;
    public int soundEffectAudioVolume = 0;
    public int masterAudioVolume = 0;
    private float maxSquareVelocity = 1250f;
    private int maxLiftPower = 120;
    private int maximumThrustInput = 22000;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.velocity = Rigidbody.rotation * new Vector3(0, 0, initialSpeed); // Sets initial velocity of the plane to the initial speed
    }

    public void SetThrottleInput(float input)
    {
        previousThrottleInput = input;
    }

    public void SetControlInput(Vector3 input)
    {
        controlInput = Vector3.ClampMagnitude(input, 1); // Clamps the value between 0-1, essentially a unit vector
    }

    public void ToggleFlaps()
    {
        if (LocalVelocity.z < flapsRetractSpeed)
        {
            FlapsDeployed = !FlapsDeployed;
            MainGameAircraftInformationUI.Instance.UpdateFlapsText();
        }
    }

    public void ToggleSpoilers()
    {
        Debug.Log("test");
        if (AirbrakeDeployed)
        {
            AirbrakeDeployed = false;
        }
        else
        {
            AirbrakeDeployed = true;
        }
        MainGameAircraftInformationUI.Instance.UpdateSpoilersText();
    }
    private void UpdateThrottle(float deltaTime)
    {
        if (throttleInput != previousThrottleInput) // If throttleInput has changed
        {
            throttleInput = previousThrottleInput;
            float target = 0;
            if (throttleInput > 0 && GameInput.Instance.playerInputActions.Plane.throttleUp.IsPressed()) // If the input is > 0 AND throttle up keybind is currently pressed...
            {
                target = 1;
                Throttle = Utility.MoveTo(Throttle, target, throttleSpeed * Mathf.Abs(throttleInput), deltaTime); // Set the throttle to the value calculated depending on scale/deltaTime
            }
            else if (throttleInput < 0.03f && GameInput.Instance.playerInputActions.Plane.throttleDown.IsPressed()) // If throttleInput < 0.03 and throttle down keybind is currently pressed...
            {
                Throttle -= 0.01f; // Decrement the throttle
                if (Throttle < 0f) // If the Throttle is < 0
                {
                    Throttle = 0f; // Reset to 0
                }
            }
            else // If neither conditions met
            {
                Throttle = Utility.MoveTo(Throttle, target, throttleSpeed * Mathf.Abs(throttleInput), deltaTime); // Set the throttle to the value calculated depending on scale/deltaTime, but the target is 0 rather than 1
            }
        }
    }

    private void UpdateFlaps()
    {
        if (LocalVelocity.z > flapsRetractSpeed) // If velocity > max flaps speed
        {
            FlapsDeployed = false; // Disable flaps
            MainGameAircraftInformationUI.Instance.UpdateFlapsText(); // Update text
        }
    }

    private void CalculateAngleOfAttack()
    {
        if (LocalVelocity.sqrMagnitude < 0.1f) // If velocity is basically 0
        {
            AngleOfAttack = 0; // Set AOA to 0
            AngleOfAttackYaw = 0;
            return;
        }

        AngleOfAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z); //Atan2 calculates the angle in between 2 directions
        AngleOfAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z);
    }

    private void CalculateGForce(float deltaTime)
    {
        var inverseRotation = Quaternion.Inverse(Rigidbody.rotation); // GForce is in opposite direction
        var acceleration = (Velocity - lastVelocity) / deltaTime; //Acceleration = change in velocity / change in time
        LocalGForce = inverseRotation * acceleration;
        lastVelocity = Velocity;
    }

    private void CalculateState()
    {
        var inverseRotation = Quaternion.Inverse(Rigidbody.rotation);
        Velocity = Rigidbody.velocity;
        LocalVelocity = inverseRotation * Velocity;
        LocalAngularVelocity = inverseRotation * Rigidbody.angularVelocity; // These lines transform the velocity into local space rather than global space (In relation to parent object)

        CalculateAngleOfAttack();
    }

    void UpdateThrust()
    {
        if (lastVelocity.sqrMagnitude > maxSquareVelocity) return;
        Rigidbody.AddRelativeForce(Throttle * maxThrust * Vector3.forward);
    }

    private void UpdateDrag()
    {
        var localVelocity = LocalVelocity;
        var localVelocitySquared = localVelocity.sqrMagnitude;

        float airbrakeDrag = AirbrakeDeployed ? this.airbrakeDrag : 0;
        float flapsDrag = FlapsDeployed ? this.flapsDrag : 0;

        var coefficient = Utility.ScalePosAndNeg(localVelocity.normalized, // Current velocity as unit vector
            dragRight.Evaluate(Mathf.Abs(localVelocity.x)), // Evaulates the animation curve at the given velocities respective x, y or z values
            dragLeft.Evaluate(Mathf.Abs(localVelocity.x)), 
            dragTop.Evaluate(Mathf.Abs(localVelocity.y)),
            dragBottom.Evaluate(Mathf.Abs(localVelocity.y)) + 0.001f, 
            dragForward.Evaluate(Mathf.Abs(localVelocity.z)) + airbrakeDrag + flapsDrag, // Need to account for the drag from applying the spoilers or flaps
            dragBack.Evaluate(Mathf.Abs(localVelocity.z))
        );

        var drag = coefficient.magnitude * localVelocitySquared * -localVelocity.normalized; // Negative because drag acts in the opposite direction to the velocity

        Rigidbody.AddRelativeForce(drag);
        if (throttleInput == 0)
        {
            Rigidbody.velocity = Rigidbody.velocity / 1.03f;
        }
    }

    private Vector3 CalculateLift(float angleOfAttack, Vector3 rightAxis, float liftPower, AnimationCurve angleOfAttackCurve, AnimationCurve inducedDragCurve)
    {
        var liftVelocity = Vector3.ProjectOnPlane(LocalVelocity, rightAxis);
        var velocitySquared = liftVelocity.sqrMagnitude;

        var liftCoefficient = angleOfAttackCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg); //Lift = velocity^2 * coefficient * lift power and need to convert radians to degrees as the curve is -90 to 90 degrees
        var liftForce = velocitySquared * liftCoefficient * liftPower;

        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis); // Lift is perpendicular to velocity
        var lift = liftDirection * liftForce;

        var dragForce = liftCoefficient * liftCoefficient;
        var dragDirection = -liftVelocity.normalized;
        var inducedDrag = dragDirection * velocitySquared * dragForce *  this.inducedDrag *inducedDragCurve.Evaluate(Mathf.Max(0, LocalVelocity.z));

        return lift + inducedDrag;
    }

    private void UpdateLift()
    {
        if (LocalVelocity.sqrMagnitude < 1f) return;

        float flapsLiftPower = FlapsDeployed ? this.flapsLiftPower : 0;
        float flapsAngleOfAttackBias = FlapsDeployed ? this.flapsAngleOfAttackBias : 0;

        var liftForce = CalculateLift(AngleOfAttack + (flapsAngleOfAttackBias * Mathf.Deg2Rad), Vector3.right, liftPower + flapsLiftPower, liftAngleOfAttackCurve, inducedDragCurve);

        var yawForce = CalculateLift(AngleOfAttackYaw, Vector3.up, rudderPower, rudderAngleOfAttackCurve, rudderInducedDragCurve);

        Rigidbody.AddRelativeForce(liftForce);
        Rigidbody.AddRelativeForce(yawForce);
    }

    private void UpdateAngularDrag()
    {
        var angularVelocity = LocalAngularVelocity;
        var drag = angularVelocity.sqrMagnitude * -angularVelocity.normalized;
        Rigidbody.AddRelativeTorque(Vector3.Scale(drag, angularDrag), ForceMode.Acceleration); // Apply the angular drag as an accelerating force
    }

    private Vector3 CalculateGForce(Vector3 angularVelocity, Vector3 velocity)
    {
        return Vector3.Cross(angularVelocity, velocity); // G = Velocity * AngularVelocity as G = Velocity^2 / R so Velocity * AngularVelocity * Radius / Radius so Velocity * AngularVelocity
    }

    private Vector3 CalculateGForceLimit(Vector3 input)
    {
        return Utility.ScalePosAndNeg(input, gForceLimit, gForceLimitPitch, gForceLimit, gForceLimit, gForceLimit, gForceLimit) * 9.81f; // Returns g force limit in all 6 directions and multiplies by gravity to get force limit
    }

    private float CalculateGlimiter(Vector3 controlInput, Vector3 maxAngularVelocity)
    {
        if (controlInput.magnitude < 0.01f) // If no input
        {
            return 1; // Return 1G as limit
        }

        var maxInput = controlInput.normalized; // Max input is the normalized value of the control input

        var limit = CalculateGForceLimit(maxInput); // Calculate the g force limit
        var maxGForce = CalculateGForce(Vector3.Scale(maxInput, maxAngularVelocity), LocalVelocity); // Calculate the max gforce given the current angular and local velocity

        if (maxGForce.magnitude > limit.magnitude) // If the max g force is greater than the limit...
        {
            return limit.magnitude / maxGForce.magnitude; // Dividing them means it can reach limit within range of input
        }

        return 1; // If not greater, return 1G
    }

    private float CalculateSteering(float deltaTime, float angularVelocity, float targetVelocity, float acceleration)
    {
        var error = targetVelocity - angularVelocity;
        var accel = acceleration * deltaTime;
        return Mathf.Clamp(error, -accel, accel);
    }

    private void UpdateSteering(float deltaTime)
    {
        var speed = Mathf.Max(0, LocalVelocity.z);
        var steeringPower = steeringCurve.Evaluate(speed); // Evaluate the AnimationCurve

        var gForceScaling = CalculateGlimiter(controlInput, turnSpeed * Mathf.Deg2Rad * steeringPower); // Scale the gForce depending on the current controlInput and max angular velocity

        var targetAngularVelocity = Vector3.Scale(controlInput, turnSpeed * steeringPower * gForceScaling); // Calculate the target angular velocity
        var angularVelocity = LocalAngularVelocity * Mathf.Rad2Deg; // Calculate the current angular velocity

        var correction = new Vector3(CalculateSteering(deltaTime, angularVelocity.x, targetAngularVelocity.x, turnAcceleration.x * steeringPower), // Calculate in x direction
                                        CalculateSteering(deltaTime, angularVelocity.y, targetAngularVelocity.y, turnAcceleration.y * steeringPower), // Calculate in y direction
                                        CalculateSteering(deltaTime, angularVelocity.z, targetAngularVelocity.z, turnAcceleration.z * steeringPower)); // Calculate in z direction
        
        Rigidbody.AddRelativeTorque(correction * Mathf.Deg2Rad, ForceMode.VelocityChange); // Add the correction as a torque

        var correctionInput = new Vector3(Mathf.Clamp((targetAngularVelocity.x - angularVelocity.x) / turnAcceleration.x, -1, 1), // Find difference, divide by the turnAcceleration which gives the timeDelta, then find between -1 and 1
                                          Mathf.Clamp((targetAngularVelocity.y - angularVelocity.y) / turnAcceleration.y, -1, 1), // Same as above for y
                                          Mathf.Clamp((targetAngularVelocity.z - angularVelocity.z) / turnAcceleration.z, -1, 1)); // Same as above for z

        var effectiveInput = (correctionInput + controlInput) * gForceScaling; // The EffectiveInput of the user is the controlInput of the user, added onto the calculated "correction" needed, multiplied by the scaling factor

        EffectiveInput = new Vector3(Mathf.Clamp(effectiveInput.x, -1, 1), // Put into x y z directions
                                    Mathf.Clamp(effectiveInput.y, -1, 1),
                                    Mathf.Clamp(effectiveInput.z, -1, 1));
    }

    public async Task UpdateUserChoices()
    {
        Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player.LoadAllAsync(); // Load all data items
        IDeserializable item;
        foreach (var saveData in savedData) // For each item
        {
            item = saveData.Value.Value;
            switch (saveData.Key) // Switch the key
            {
                case "Difficulty": // If the key's named "Difficulty"...
                    if (item.GetAs<string>() == "Simple") // Fetch it's value, and check if its "Simple"
                    {
                        gForceLimit = 5;
                        turnSpeed = new Vector3(20, 10, 180);
                        turnAcceleration = new Vector3(20, 10, 180);
                        inducedDrag = 90;
                        break;
                    }
                    else
                    {
                        break;
                    }
                case "TerrainHeight": // If the key's named "TerrainHeight"
                    HeightMapSettings.Instance.heightMultiplier = 60 - ((11 - item.GetAs<float>()) * 4);
                    break;
                case "MasterAudio":
                    masterAudioVolume = item.GetAs<int>();
                    break;
                case "SoundEffectAudio":
                    soundEffectAudioVolume = item.GetAs<int>();
                    break;
                case "AircraftSelection":
                    Debug.Log(item.GetAsString());
                    switch (item.GetAsString())
                    {
                        case "Typhoon": // Check the given text against each possible aircraft
                            AircraftSoundEffectManager.Instance.soundEffectAudioSource.clip = AircraftSoundEffectManager.Instance.soundEffectChoices[0];
                            AircraftSoundEffectManager.Instance.soundEffectAudioSource.Play();
                            break;
                        case "Eagle": // Same as above
                            AircraftSoundEffectManager.Instance.soundEffectAudioSource.clip = AircraftSoundEffectManager.Instance.soundEffectChoices[0];
                            AircraftSoundEffectManager.Instance.soundEffectAudioSource.Play();
                            break;
                        case "Business Jet": // Same as above
                            AircraftSoundEffectManager.Instance.soundEffectAudioSource.clip = AircraftSoundEffectManager.Instance.soundEffectChoices[1];
                            AircraftSoundEffectManager.Instance.soundEffectAudioSource.Play();
                            maximumThrustInput = 30000;
                            maxSquareVelocity = 2500;
                            break;
                        case "Rafale": // Same as above
                            AircraftSoundEffectManager.Instance.soundEffectAudioSource.clip = AircraftSoundEffectManager.Instance.soundEffectChoices[2];
                            AircraftSoundEffectManager.Instance.soundEffectAudioSource.Play();
                            maximumThrustInput = 30000;
                            maxSquareVelocity = 5000;
                            maxLiftPower = 140;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        CalculateState();
        CalculateGForce(deltaTime);
        UpdateFlaps();

        UpdateThrottle(deltaTime);

        UpdateThrust();
        UpdateLift();
        UpdateSteering(deltaTime);

        UpdateDrag();
        UpdateAngularDrag();
    }

    private void Update()
    {
        liftPower = (maxLiftPower - Mathf.RoundToInt(transform.position.y));
        maxThrust = (maximumThrustInput - (Mathf.RoundToInt(transform.position.y) * 50));
    }
}
