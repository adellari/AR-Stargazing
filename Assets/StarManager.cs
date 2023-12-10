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
    }

    //Set skyDisplay before calling these
    public IEnumerator toggleSegment()
    {
        yield return null;
    }
    
    public IEnumerator togglDome()
    {
        yield return null;
    }
    
    //0-> 360, 0-> 360, 0-> 360
    //540 is the middle of the scrollview 
    // map 360 -> 720 
    
    public IEnumerator toggleHemi()
    {
        //determine our start and end values based on whether we're already in this state
        float sV = projection == skyDisplay.hemi ? 0f : 1f; //start value
        float eV = projection == skyDisplay.hemi ? 1f : 0f; //end value

        float tt = 3f; //transition time
        float st = Time.realtimeSinceStartup; //start time

        while (Time.realtimeSinceStartup - st < tt)
        {
            skyOpacity = Mathf.Lerp(sV, eV, (Time.realtimeSinceStartup - st) / tt);
            yield return null;
        }

        skyOpacity = eV;
        yield return null;
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
