using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine;
using AOT;

/// <summary>
/// C-API exposed by the Host, i.e., Unity -> Host API.
/// </summary>
public class HostNativeAPI {
    public delegate void TestDelegate(string name);
    public delegate void ProjectionDelegate(int flag);
    public delegate void WavelengthDelegate(int flag);
    
    
    [DllImport("__Internal")]
    public static extern void sendUnityStateUpdate(string state);
    
    [DllImport("__Internal")]
    public static extern void sendCompassUpdate(float val);

    [DllImport("__Internal")]
    public static extern void setTestDelegate(TestDelegate cb);
    
    [DllImport("__Internal")]
    public static extern void setProjectionDelegate(ProjectionDelegate cb);

    [DllImport("__Internal")]
    public static extern void setWavelengthDelegate(WavelengthDelegate cb);
}

/// <summary>
/// C-API exposed by Unity, i.e., Host -> Unity API.
/// </summary>
public class UnityNativeAPI 
{
    public static StarManager _StarManager;
    
    [MonoPInvokeCallback(typeof(HostNativeAPI.TestDelegate))]
    public static void test(string name) {
        Debug.Log("This static function has been called from iOS!");
        Debug.Log(name);
    }
    
    [MonoPInvokeCallback(typeof(HostNativeAPI.ProjectionDelegate))]
    public static void setProjection(int flag)
    {
        Debug.Log($"Set the projection flag to {flag}");
        _StarManager.toggleSkyChange(flag);
    }

    [MonoPInvokeCallback(typeof(HostNativeAPI.WavelengthDelegate))]
    public static void setWavelength(int flag)
    {
        Debug.Log($"Received Swift set wavelength to {flag}");
        _StarManager.onToggleWavelength(flag);
    }
    

}

public class API : MonoBehaviour
{
    public StarManager _StarManager;
    public AstroCompass Compass;
    void Start()
    {
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            UnityNativeAPI._StarManager = _StarManager;
            HostNativeAPI.setTestDelegate(UnityNativeAPI.test);
            HostNativeAPI.sendUnityStateUpdate("ready");
            HostNativeAPI.setProjectionDelegate(UnityNativeAPI.setProjection);
            HostNativeAPI.setWavelengthDelegate(UnityNativeAPI.setWavelength);
            
        }
#endif
    }

    void Update()
    {
        #if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            HostNativeAPI.sendCompassUpdate(Compass.Heading);
        }
        
        #endif
    }
}