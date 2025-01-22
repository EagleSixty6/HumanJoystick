using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonBehaviour : MonoBehaviour
{
    public GameObject _treasure;
    public float _timeDeltaBetweenDrops;
    public List<GameObject> _waypointsGO;
    public GameObject _playerPlatform;
    public LayerMask _terrainLayer;

    public enum state
    {
        idle,
        movingTo
    }

    private float timeDeltaLastDrop;
    private state currentState = state.idle;
    private int currentWaypoint = -1;
    private float velocity = 3f;
    private float degreePerSecond = 200f;

    void Start()
    {
        GetComponent<Animation>().wrapMode = WrapMode.Loop;
        GetComponent<Animation>().CrossFade("Idle");
        timeDeltaLastDrop = _timeDeltaBetweenDrops;
    }
    
    void Update()
    {
        //check distance to player; when player close move away
        Vector3 lineOfSight = _playerPlatform.transform.position - transform.position;

        if (lineOfSight.magnitude < 4 && currentState != state.movingTo && currentWaypoint + 1 < _waypointsGO.Count)
        {
            currentWaypoint++;
            currentState = state.movingTo;
            GetComponent<Animation>().CrossFade("Walk");
        }
        
        if (currentState == state.movingTo)
        {
            RaycastHit hitEnd, hitStart;
            int layerMask = 1 << 8; // terrain
            Physics.Raycast(  new Vector3(_waypointsGO[currentWaypoint].transform.position.x, 100, _waypointsGO[currentWaypoint].transform.position.z), -Vector3.up, out hitEnd, Mathf.Infinity, layerMask);
            Physics.Raycast(  new Vector3(transform.position.x,100,transform.position.z), -Vector3.up, out hitStart, Mathf.Infinity, layerMask);

            Vector3 diff = hitEnd.point - hitStart.point;
            Vector3 translation = Time.deltaTime * velocity * diff.normalized;
            if (diff.magnitude <= translation.magnitude)
            {
                this.transform.Translate(diff, Space.World);
                currentState = state.idle;
                GetComponent<Animation>().CrossFade("Idle");
            }
            else
            {
                this.transform.Translate(translation, Space.World);
            }
            
            SmoothRotateTo(diff);
            
            // drop a treasure?
            if (timeDeltaLastDrop >= _timeDeltaBetweenDrops)
            {
                DropTreasure();
                timeDeltaLastDrop = 0;
            }
            timeDeltaLastDrop += Time.deltaTime;
        }
        else if(currentState == state.idle)
        {
            SmoothRotateTo(lineOfSight);
        }

        RaycastHit terrainHit;
        Physics.Raycast(transform.position + Vector3.up * 100f, Vector3.down, out terrainHit, Mathf.Infinity, _terrainLayer);
        transform.position = terrainHit.point + Vector3.up * 0.5f;
    }

    private void SmoothRotateTo(Vector3 lineOfSight)
    {
        lineOfSight.y = 0;
        float angleDiff = Vector3.SignedAngle(transform.forward, lineOfSight, Vector3.up);

        if (Mathf.Sign(angleDiff) < 0)
        {
            this.transform.Rotate(Vector3.up, Mathf.Max(-degreePerSecond * Time.deltaTime, angleDiff));
        }
        else
        {
            this.transform.Rotate(Vector3.up, Mathf.Min(degreePerSecond * Time.deltaTime, angleDiff));
        }
    }

    private void DropTreasure()
    {
        Instantiate(_treasure, transform.position + new Vector3(0,0.4f,0), Quaternion.identity);
    }
}
