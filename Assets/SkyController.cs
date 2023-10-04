using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyController : MonoBehaviour
{
    public Transform Polaris;
    Transform Sphere;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setNorth(Vector3 heading)
    {
        transform.LookAt(heading* 1000f);

    }


    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.main.transform.position;
    }
}
