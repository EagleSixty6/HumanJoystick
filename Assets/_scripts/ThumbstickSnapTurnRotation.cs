using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbstickSnapTurnRotation : MonoBehaviour
{
    [Tooltip("The frequency in which a turn is performed when not returning the stick to neutral position. The neutral position resets the cooldown.")]
    public float _jumpFrequency = 0.2f;

    [Tooltip("Rotation in degree.")]
    public float _rotationStep = 12.5f;

    private float _snapTurnCoolDown = 0f;

    void Update()
    {
        float primaryThumbstickX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        float secondaryThumbstickX = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        float largerValue = Mathf.Abs(primaryThumbstickX) > Mathf.Abs(secondaryThumbstickX)
            ? primaryThumbstickX
            : secondaryThumbstickX;

        if (largerValue == 0)
        {
            _snapTurnCoolDown += _jumpFrequency;
        }
        else if (_snapTurnCoolDown >= _jumpFrequency)
        {
            _snapTurnCoolDown = 0;


            if (largerValue > 0)
            {
                transform.Rotate(Vector3.up, _rotationStep);
            }
            else if (largerValue < 0)
            {
                transform.Rotate(Vector3.up, -_rotationStep);
            }
        }
        _snapTurnCoolDown += Time.deltaTime;
    }
}
