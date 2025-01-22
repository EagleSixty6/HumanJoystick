using System;
using UnityEngine;

public class HumanJoystickTranslation : MonoBehaviour
{
    #region Public Fields
    [Header("Locomotion Settings")]
    [Tooltip("Changes the maximum speed in m/s of forward and backward translation.")]
    public float _maxTranslationSpeed = 3f;

    [Tooltip("Leaning forward dead-zone in percent.")]
    [Range(0f, 0.9f)]
    public float _deadzone = 0f;

    [Tooltip("Leaning forward dead-zone in percent.")]
    public float _offsetHeadPivot = 0.09f;

    [Tooltip("Power of the exponetial Function")]
    [Range(1f, 2f)]
    private float _exponentialTransferFunctionPower = 1.53f;

    [Tooltip("Define the distance from center which results in maximum axis deviation.")]
    public float _bodyOffsetForMaxSpeed = 0.4f;

    public LayerMask _terrainLayer;

    [Header("Transfer Function")]
    [Tooltip("Sensitivity of leaning (inside the exponential function)")]
    [Range(0f, 5f)]
    public float _transferSensitivity = 1f;

    [Tooltip("(outside of the exponential function)")]
    [Range(0f, 10f)]
    // In Summary --> input = transferFactor * (leaningMag * speedSensitivity)^(exponential)
    public float _transferFactor = 1f;

    #endregion

    GameObject _headJoint;
    private GameObject _camera;
    private Vector3 _leaningRefPosition = Vector3.zero;
    private float _velocityAxis = 0;
    private Vector3 _tiltingDirectionLocal;
    private float _lastTriggerState = 0;
    
    void Start()
    {
        _camera = GameObject.Find("CenterEyeAnchor");
        GameObject centerOfYawRotation = new GameObject("CenterOfYawRotation");
        Vector3 centerOfYawRotationPosition = _camera.transform.position - (_camera.transform.forward * _offsetHeadPivot);
        _headJoint = Instantiate(centerOfYawRotation, centerOfYawRotationPosition, Quaternion.identity, _camera.transform);
    }

    void Update()
    {
        OVRInput.Update();
        float triggerAxis = MathF.Max(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger), OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger));

        // calibrate leaning each time the interface is activated
        if (OnTriggerDown(triggerAxis))
        {
            CalibrateLeaningKS();
        }

        UpdateLeaningInputs();

        float distanceToTravel = _velocityAxis * _maxTranslationSpeed;
        transform.position += distanceToTravel * Time.deltaTime * GetMovementDirection() * triggerAxis;

        MoveTransformToTerrain();
    }

    // Set the reference point for the leaning interface
    public void CalibrateLeaningKS()
    {
        _leaningRefPosition = this.transform.InverseTransformPoint(_headJoint.transform.position);
    }
    
    // I do not want to use gravity in VR because of potentially awkward physics behaviour, thus we have to manually set the platform to the ground for ground based travel
    private void MoveTransformToTerrain()
    {
        RaycastHit terrainHit;
        Physics.Raycast(transform.position + Vector3.up * 100f, Vector3.down, out terrainHit, Mathf.Infinity, _terrainLayer);
        transform.position = terrainHit.point;
    }

    private void UpdateLeaningInputs()
    {
        Vector3 diff = this.transform.InverseTransformPoint(_headJoint.transform.position) - _leaningRefPosition;
        _velocityAxis = diff.magnitude;
        diff = Vector3.ProjectOnPlane(diff, Vector3.up);
        _tiltingDirectionLocal = diff.normalized;
        
        // clamp body tilt to an axis
        _velocityAxis = Mathf.Clamp(_velocityAxis / _bodyOffsetForMaxSpeed, 0, 1);
        
        // apply transform function
        float transformFunc = Mathf.Pow(Mathf.Max(0, _velocityAxis - _deadzone) * _transferSensitivity, _exponentialTransferFunctionPower) * _transferFactor;
        
        _velocityAxis = _velocityAxis * transformFunc;
    }
    
    public Vector3 GetMovementDirection()
    {
        return transform.localToWorldMatrix * _tiltingDirectionLocal;
    }

    public Vector2 GetAxis2D()
    {
        Vector3 axis3D = _velocityAxis * _tiltingDirectionLocal;
        return new Vector2(axis3D.x, axis3D.z);
    }

    private Boolean OnTriggerDown(float val)
    {
        if (val > 0f && _lastTriggerState == 0f)
        {
            _lastTriggerState = val;
            return true;
        }

        _lastTriggerState = val;
        return false;
    }
}
