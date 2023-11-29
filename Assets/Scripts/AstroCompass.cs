using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Niantic.Lightship.AR.Samples;

public class AstroCompass : MonoBehaviour
{
    public SkyController dome;
    public bool isLoc = false;

    [Header("Debug")]
    private LineRenderer lr;
    public TMP_Text debugText;
    public TMP_Text debugText1;
    public Transform Sphere;
    public Transform debugBall;
    public Camera skyBoxCam;

    private float bestAccuracy;
    private Vector3 polarisDirection;
    private Quaternion lastOrientation;
    private Vector3 lastForward;

    public RawImage debugQuad;
    public RawImage debugQuad2;
    public AstroSegmentation semanticObj;

    public IEnumerator Start()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthMultiplier = 0.03f;

        Input.location.Start();
        Input.gyro.enabled = true;
        Input.compass.enabled = true;

        skyBoxCam.fieldOfView = Camera.main.fieldOfView;

        var locWait = 10;
        while (Input.location.status == LocationServiceStatus.Initializing && locWait > 0)
        {
            yield return new WaitForSeconds(1);
            locWait--;
        }

        if (locWait < 1)
        {
            Debug.LogWarning("Timed out waiting for location services!");
            isLoc = false;
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Failed initializing location services");
            isLoc = false;
            yield break;
        }
        else
        {
            /*
             
            isLoc = true;
            altitude = Input.location.lastData.altitude;
            latitude = Input.location.lastData.latitude;

            if (isLoc)
            {
                Az = Input.compass.trueHeading;
                pAz = Az;
            }
                
            
            Debug.Log("Azimuth: " + Az);
            Debug.Log("Successfully obtained geographic data");
            
             */
        }

        
        //northDir.z = 0;
        //northDir.Normalize();
        polarisDirection = NorthVector();
        bestAccuracy = Input.compass.headingAccuracy;
        lastOrientation = transform.rotation;
        lastForward = transform.forward;
        // Input.location.Stop();
        isLoc = true;
        StartCoroutine(calibrate());

    }

    void periodicDebug()
    {
        /*
        Debug.Log("Main camera FOV: " + Camera.main.fieldOfView);
        Debug.Log("Secondary camera FOV: " + skyBoxCam.fieldOfView);


        Debug.Log("Main camera rotation: " + Camera.main.transform.eulerAngles);
        Debug.Log("Secondary camera rotation: " + skyBoxCam.transform.eulerAngles);

        Debug.Log("Main camera flags: " + Camera.main.clearFlags);
        */

        //Debug.Log("skybox camera enabled: " + skyBoxCam.enabled);
    }

    private IEnumerator calibrate()
    {
        var val = NorthVector();
        var acc = Input.compass.headingAccuracy;
        if (acc >= bestAccuracy)
        {
            
            bestAccuracy = acc;
            polarisDirection = val;
        }

        
        periodicDebug();


        yield return new WaitForSecondsRealtime(7);
        StartCoroutine(calibrate());
    }

    Vector3 NorthVector()
    {
        //multiply our camera's current forward vector with a planar rotation towards north
        Vector3 northDir = Quaternion.Euler(0f, -Input.compass.trueHeading, 0f) * transform.forward;

        //need to multiply this by another rotation towards our latitudinal angle
        northDir = Quaternion.AngleAxis(Input.location.lastData.latitude, -transform.right) * northDir;

        return northDir * 1000000f;
    }

    void GetAltitude()
    {
        Quaternion deviceRotation = Input.gyro.attitude;
        deviceRotation = Quaternion.Euler(90f, 0f, 0f) * new Quaternion(deviceRotation.x, deviceRotation.y, -deviceRotation.z, -deviceRotation.w);

        float pitch = deviceRotation.eulerAngles.x;

        float altitude;
        if (pitch <= 180f)
            altitude = 90f - pitch;
        else altitude = pitch - 270f;

        altitude = -pitch;

        Debug.Log("Altitude: " + altitude);
    }


    void GetAzimuth()
    {
        /*
         
         
        Quaternion deviceRotation = Input.gyro.attitude;
        Vector3 deviceUpWorld = deviceRotation * Vector3.up;

        deviceRotation = Quaternion.Euler(90f, 0f, 0f) * new Quaternion(deviceRotation.x, deviceRotation.y, -deviceRotation.z, -deviceRotation.w);
        Vector3 projected = Vector3.ProjectOnPlane(deviceUpWorld, Vector3.up);

        Az = Vector3.SignedAngle(Vector3.forward, projected, Vector3.up);


        Debug.Log("Azimuth: " + Az);

        */
        //Debug.Log("Fake azimuth: " + Input.compass.magneticHeading);
    }


    //Need to implement orientation checks as is done in astroSegmentation 
    Vector2 screenSize()
    {
        var aspect = Mathf.Max(Camera.main.pixelWidth, Camera.main.pixelHeight) / (float)Mathf.Min(Camera.main.pixelWidth, Camera.main.pixelHeight);

        float minDimension = Camera.main.pixelWidth;
        float maxDimension = Mathf.Round(minDimension * aspect);

        Vector2 screenRect = new Vector2(minDimension, maxDimension);

        return screenRect;
    }


    private void Update()
    {

        Vector2 screenRect = screenSize();

        Camera.main.clearFlags = CameraClearFlags.Depth;
        skyBoxCam.clearFlags = CameraClearFlags.Skybox;
        skyBoxCam.fieldOfView = Camera.main.fieldOfView;
        Matrix4x4 camToWorld = Camera.main.cameraToWorldMatrix;

        debugQuad.rectTransform.sizeDelta = screenRect;
        debugQuad2.rectTransform.sizeDelta = screenRect;
        //debugQuad.material.SetMatrix("_InverseViewMatrix", camToWorld);
        //debugQuad.material.SetTexture("_SemanticMask", semanticObj._texture);
        //debugQuad.material.SetMatrix("_DisplayMatrix", semanticObj.displayMatrix);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isLoc)
        {
            //polarisDirection = Quaternion.FromToRotation(lastForward, transform.forward) * polarisDirection;
            //polarisDirection = (transform.rotation * Quaternion.Inverse(lastOrientation)) * polarisDirection;
            //polarisDirection = NorthVector();
            lastForward = transform.forward;
            lastOrientation = transform.rotation;

            lr.SetPosition(0, transform.forward * 1.1f + transform.position);
            lr.SetPosition(1, polarisDirection * 5f);

            debugBall.position = polarisDirection * 5f;
            debugText.text = Input.compass.trueHeading.ToString();

            dome.setNorth(polarisDirection);

            debugText1.text = Input.compass.headingAccuracy.ToString();
            //Debug.Log("set look vector: " + Input.compass.trueHeading);
        }
        


    }

    private void OnApplicationQuit()
    {
        Input.location.Stop();
        Input.gyro.enabled = false;
        Input.compass.enabled = false;
    }

    private void OnApplicationFocus(bool focus)
    {
        //when refocused, recalibrate polaris
    }
}
