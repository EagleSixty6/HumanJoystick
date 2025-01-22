using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finished : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PlayerPlatform")
        {
            transform.GetChild(0).GetComponent<ParticleSystem>().Play();
            GetComponent<AudioSource>().Play();
        }
    }
}
