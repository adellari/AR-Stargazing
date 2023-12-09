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
    
    public Material skybox;
    [Range(0, 360f)]
    public float skyboxRotation = 0;

    private Matrix4x4 skyboxCorrection;
    [SerializeField]
    public List<starmap> Starmaps;

    [Range(0f, 1f)]
    public float skyOpacity;
    void Start()
    {
        
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
    
    public IEnumerator enabledSemidome

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
