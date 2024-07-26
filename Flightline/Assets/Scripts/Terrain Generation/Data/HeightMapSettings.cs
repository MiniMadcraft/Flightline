using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HeightMapSettings : UpdatableData
{
    public NoiseSettings noiseSettings;

    public static HeightMapSettings Instance { get; private set; }

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }

    // do not compile if not in the unity editor otherwise the "override" does not make sense in relation to updatabledata
    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }
#endif

    private void Awake()
    {
        Instance = this;
    }
}
