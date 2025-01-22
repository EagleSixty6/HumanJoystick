using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRotation : MonoBehaviour
{
    public float rotationSpeed = 100.0f;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}