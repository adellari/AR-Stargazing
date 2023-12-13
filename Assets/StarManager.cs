using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StarManager : MonoBehaviour
{
    [System.Serializable]
    public class starmap
    {
        public string name;
        public string propertyName;
        public bool active;
        [Range(0f, 1f)]
        public float blendFactor;

        public void setProperty(Material mat, float val)
        {
            mat.SetFloat(propertyName, val);
            
        }
    }

    public enum skyDisplay
    {
        dome,
        hemi,
        segment,
        none
    }
    
    public Material skybox;
    [Range(0, 360f)]
    public float skyboxRotation = 0;

    private Matrix4x4 skyboxCorrection;
    [SerializeField]
    public List<starmap> Starmaps;

    [Range(0f, 1f)]
    public float skyOpacity;

    public skyDisplay projection;
    public AstroSegmentation segmentationManager;
    void Start()
    {
        var rot = Quaternion.AngleAxis(skyboxRotation, Vector3.forward);
        skyboxCorrection = Matrix4x4.Rotate(rot);
        projection = skyDisplay.none;
    }

    //Set skyDisplay before calling these
    public IEnumerator toggleSegment()
    {
        //determine our start and end values based on whether we're already in this state
        float sV = projection != skyDisplay.segment ? 0f : 1f; //start value
        float eV = projection != skyDisplay.segment ? 1f : 0f; //end value

        float tt = 1.5f; //transition time
        float st = Time.realtimeSinceStartup; //start time

        while (Time.realtimeSinceStartup - st < tt)
        {
            skyOpacity = Mathf.Lerp(sV, eV, (Time.realtimeSinceStartup - st) / tt);
            yield return null;
        }

        skyOpacity = eV;
        projection = projection == skyDisplay.segment? skyDisplay.none : skyDisplay.segment;
        if (eV == 1) 
            segmentationManager.toggleSemanticMode(true);
        else
            segmentationManager.toggleOff(false);
        Debug.Log("finished setting segment coroutine");
        yield return null;
    }
    
    public IEnumerator toggleDome()
    {
        //determine our start and end values based on whether we're already in this state
        float sV = projection != skyDisplay.dome ? 0f : 1f; //start value
        float eV = projection != skyDisplay.dome ? 1f : 0f; //end value

        float tt = 1.5f; //transition time
        float st = Time.realtimeSinceStartup; //start time

        while (Time.realtimeSinceStartup - st < tt)
        {
            skyOpacity = Mathf.Lerp(sV, eV, (Time.realtimeSinceStartup - st) / tt);
            yield return null;
        }

        skyOpacity = eV;
        projection = projection == skyDisplay.dome? skyDisplay.none : skyDisplay.dome;
        Debug.Log("finished setting full dome coroutine");
        yield return null;
    }
    
    //0-> 360, 0-> 360, 0-> 360
    //540 is the middle of the scrollview 
    // map 360 -> 720 
    
    public IEnumerator toggleHemi()
    {
        //determine our start and end values based on whether we're already in this state
        float sV = projection != skyDisplay.hemi ? 0f : 1f; //start value
        float eV = projection != skyDisplay.hemi ? 1f : 0f; //end value

        float tt = 1.5f; //transition time
        float st = Time.realtimeSinceStartup; //start time

        while (Time.realtimeSinceStartup - st < tt)
        {
            skyOpacity = Mathf.Lerp(sV, eV, (Time.realtimeSinceStartup - st) / tt);
            yield return null;
        }

        skyOpacity = eV;
        projection = projection == skyDisplay.hemi? skyDisplay.none : skyDisplay.hemi;
        Debug.Log("finished setting hemisphere coroutine");
        yield return null;
    }

    public IEnumerator clearSky(int nextCall)
    {
        
        Debug.Log("clearing the sky before making another call");
        float sV = 1f;
        float eV = 0f;

        float tt = 0.8f;
        float st = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - st < tt)
        {
            skyOpacity = Mathf.Lerp(sV, eV, (Time.realtimeSinceStartup - st) / tt);
            yield return null;
        }

        skyOpacity = eV;
        
        switch (nextCall)
        {
            case 0:
                segmentationManager.isHemisphere = 0;
                StartCoroutine(toggleDome());
                break;
            case 1:
                segmentationManager.isHemisphere = 1;
                StartCoroutine(toggleHemi());
                break;
            case 2:
                segmentationManager.isHemisphere = -1;
                StartCoroutine(toggleSegment());
                break;
            case -1:
                segmentationManager.isHemisphere = 0;
                break;
        }

        yield return null;
    }

    public void toggleSkyChange(int flag)
    {
        switch (flag)
        {
            case 0:
                if (projection == skyDisplay.hemi || projection == skyDisplay.segment)
                {
                    StartCoroutine(clearSky(0));
                    break;
                }
                segmentationManager.isHemisphere = 0;
                StartCoroutine(toggleDome());
                break;
            case 1:
                if (projection == skyDisplay.dome || projection == skyDisplay.segment)
                {
                    StartCoroutine(clearSky(1));
                    break;
                }
                segmentationManager.isHemisphere = 1;
                StartCoroutine(toggleHemi());
                break;
            case 2:
                if (projection == skyDisplay.hemi || projection == skyDisplay.dome)
                {
                    StartCoroutine(clearSky(2));
                    break;
                }

                segmentationManager.isHemisphere = -1;
                StartCoroutine(toggleSegment());
                break;
        }
    }

    public void onChangeSlider1(float val)
    {
        if (Starmaps != null)
        {
            if (Starmaps.Count > 0)
                Starmaps[0].blendFactor = val;
        }
    }

    public void onOpacityChange(float val)
    {
        skyOpacity = val;
    }

    public void onAngleChange(float val)
    {
        skyboxRotation = val;
        var rot = Quaternion.AngleAxis(val, Vector3.forward);
        skyboxCorrection = Matrix4x4.Rotate(rot);
    }

    // Update is called once per frame
    void Update()
    {
        if (Starmaps != null)
        {
            Starmaps.Where(a => a.active).ToList().ForEach(a => a.setProperty(skybox, a.blendFactor));
            Starmaps.Where(a => !a.active).ToList().ForEach(a => a.setProperty(skybox, 0f));
        }
        skybox.SetFloat("_opacity", skyOpacity);
        skybox.SetMatrix("_starCorrection", skyboxCorrection);
    }
}
