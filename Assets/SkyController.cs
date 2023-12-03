using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyController : MonoBehaviour
{
    public Transform Polaris;


    public void setNorth(Vector3 heading)
    {
        transform.LookAt(heading* 1000f);

    }

    public void alignNorth(float heading, float incl)
    {
        float deg = -incl - Camera.main.transform.eulerAngles.x; //subtract because we expect it to be negative
        Debug.Log($"aligning north indicator, latitude {incl}, camera incline {Camera.main.transform.eulerAngles.x}");
        Polaris.transform.localEulerAngles = new Vector3(90 + deg, 0f, heading); //incl modifies x 
    }


    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.main.transform.position;
    }
}
