using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonProxyBehaviour : MonoBehaviour
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

    void Start()
    {
    
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
            }
            else
            {
                this.transform.Translate(translation, Space.World);
            }
             
            // drop a treasure?
            if (timeDeltaLastDrop >= _timeDeltaBetweenDrops)
            {
                DropTreasure();
                timeDeltaLastDrop = 0;
            }
            timeDeltaLastDrop += Time.deltaTime;
        }

        RaycastHit terrainHit;
        Physics.Raycast(transform.position + Vector3.up * 100f, Vector3.down, out terrainHit, Mathf.Infinity, _terrainLayer);
        transform.position = terrainHit.point + Vector3.up * 0.5f;
    }

    private void DropTreasure()
    {
        Instantiate(_treasure, transform.position + new Vector3(0,0.4f,0), Quaternion.identity);
    }
}
