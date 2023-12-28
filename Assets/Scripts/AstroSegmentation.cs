using System.Linq;
using Niantic.Lightship.AR.Semantics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;


    public class AstroSegmentation : MonoBehaviour
    {

        public enum SegmentationStates
        {
            statusCheck,
            activeSky,
            off
        }
        
        const string k_DisplayMatrixName = "DisplayMatrix";
        public ARSemanticSegmentationManager segmentationManager;
        public AROcclusionManager occlusionManager;
        private readonly int k_DisplayMatrix = Shader.PropertyToID(k_DisplayMatrixName);

        [Header("Render to Debug")]
        protected ScreenOrientation m_CurrentScreenOrientation;
        [SerializeField] protected RawImage m_RawImage;
        [SerializeField] protected RawImage m_RawImage2;
        [SerializeField] protected RawImage m_WireImage;
        [SerializeField] protected RawImage m_maskImage;
        [SerializeField] protected RawImage m_Stars;
        public int isHemisphere = 0;
        
        [Header("Segmenation & Filtering Params")]
        [SerializeField] Material m_Material;
        // The rendering Unity camera
        private Camera m_camera;
        private float tanFov;
        
        public Texture _skyTexture;
        public Texture _groundTexture;
        public Matrix4x4 displayMatrix; //needs to be transposed on android
        private Matrix4x4 _displayMatrix;
        private bool segmentationReady = false;
        private RenderTexture _previousFrame;
        //private XRCpuImage? _depthImage;
            
        [Header("Compute")] 
        [SerializeField] private RenderTexture JFA_Mask;

        [SerializeField] private RenderTexture scaledSegmentation;
        [SerializeField] private ComputeShader compute;

        [SerializeField] private SegmentationStates activeState;
        private bool JFA_enabled = true;
        private float JFA_smoothing = 0.02927906f;
        private float JFA_exp = 1.267775f;
        private float confidenceThresh = 0.16f;
        private float startedTime = 0f;

        public void onToggleChange(bool val)
        {
            JFA_enabled = val;
        }

        public void onThresholdChange(float val)
        {
            confidenceThresh = val;
            Debug.Log($"set the threshold value to {confidenceThresh}");
        }

        public void onSmoothingChange(float val)
        {
            JFA_exp = val;
            Debug.Log($"set the exp value to {JFA_exp}");
        }

        public void toggleSemanticMode(bool state)
        {
            if (state)
            {
                activeState = SegmentationStates.activeSky;
            }
            else
                activeState = SegmentationStates.statusCheck;
        }

        //cannot be used to turn on sky segmentation, only go from statusCheck <-> off
        public void toggleOff(bool state)
        {
            if (activeState == SegmentationStates.off)
            {
                if (!state)
                    return;
                activeState = SegmentationStates.statusCheck;
            }
            else
            {
                if (state)
                    return;
                activeState = SegmentationStates.off;
            }
            
        }
            
        void Awake()
        {

            // Acquire a reference to the rendering camera
            m_camera = Camera.main;
            segmentationManager = m_camera.GetComponent<ARSemanticSegmentationManager>()?? m_camera.AddComponent<ARSemanticSegmentationManager>();
            occlusionManager = m_camera.GetComponent<AROcclusionManager>() ?? m_camera.AddComponent<AROcclusionManager>(); //this will be disabled if not in use/being tested
            segmentationManager.MetadataInitialized += OnSemanticModelReady;
            

            // Get the current screen orientation, and update the raw image UI
            m_CurrentScreenOrientation = Screen.orientation;
            startedTime = Time.realtimeSinceStartup;
            toggleOff(false);
            Debug.Log($"Turned segmentation manager off: {activeState == SegmentationStates.off}");
        }

        void OnEnable()
        {
            m_camera.GetComponent<ARCameraManager>().frameReceived += OnFrameReceived;
            UpdateRawImage();
        }

        void OnDisable()
        {
            m_camera.GetComponent<ARCameraManager>().frameReceived -= OnFrameReceived;
        }

        void OnFrameReceived(ARCameraFrameEventArgs args)
        {
            _displayMatrix = args.displayMatrix ?? Matrix4x4.identity;
        }

        private void OnSemanticModelReady(ARSemanticSegmentationModelEventArgs args)
        {
            segmentationReady = true;
        }

        void displaySegmentation()
        {
            
            m_RawImage2.texture = scaledSegmentation;
            m_RawImage2.material.SetTexture("_SemanticMask", scaledSegmentation);
            m_RawImage2.material.SetMatrix("_DisplayMatrix", displayMatrix);
           
        }

        void performSegmentation(XRCameraParams viewport)
        {
            _skyTexture = segmentationManager.GetSemanticChannelTexture("sky", out displayMatrix, viewport);
            _groundTexture = segmentationManager.GetSemanticChannelTexture("ground", out displayMatrix, viewport);
            //m_RawImage.texture = _skyTexture;

            if (!_skyTexture || !_groundTexture)
                return;

            if (!JFA_Mask || scaledSegmentation == null)
            {
                //var dim = new Vector2Int(_skyTexture.width, _skyTexture.height);
                var dim = new Vector2Int(m_camera.pixelWidth, m_camera.pixelHeight);
                JFA_Mask = new RenderTexture(dim.x, dim.y, 0);
                JFA_Mask.enableRandomWrite = true;
                JFA_Mask.Create();
                
                
                scaledSegmentation = new RenderTexture(m_camera.pixelWidth, m_camera.pixelHeight, 0);
                scaledSegmentation.enableRandomWrite = true;
                scaledSegmentation.Create();
            }
            
            
            DispatchBilinear(_skyTexture);
            if(JFA_enabled)
                DispatchFlood(scaledSegmentation);

            displaySegmentation();
        }
        
        void Update()
        {
            

            if (m_CurrentScreenOrientation != Screen.orientation)
            {
                m_CurrentScreenOrientation = Screen.orientation;
                UpdateRawImage();
            }
            
            m_RawImage2.material.SetFloat("_AspectRatio", m_camera.aspect);
            m_RawImage2.material.SetFloat("_TanFov", tanFov);
            m_RawImage2.material.SetMatrix("_InverseViewMatrix", m_camera.cameraToWorldMatrix);
            m_RawImage2.material.SetInteger("isHemisphere", isHemisphere);
            
            m_Stars.material.SetFloat("_aspectRatio", m_camera.aspect);
            m_Stars.material.SetFloat("_tanFOV", tanFov);
            m_Stars.material.SetMatrix("_InverseViewMatrix", m_camera.cameraToWorldMatrix);
            
            m_WireImage.material.SetFloat("_aspectRatio", m_camera.aspect);
            m_WireImage.material.SetFloat("_tanFOV", tanFov);
            m_WireImage.material.SetMatrix("InverseViewMatrix", m_camera.cameraToWorldMatrix);

            
            
            // Update the image
            var sizeDelta = m_RawImage.rectTransform.sizeDelta;
            var viewport = new XRCameraParams
            {
                screenWidth = (int)(sizeDelta.x), 
                screenHeight = (int)sizeDelta.y,
                screenOrientation = m_CurrentScreenOrientation
            };

            if (!segmentationReady)
                return;
            
            
            if(activeState == SegmentationStates.activeSky)
                performSegmentation(viewport);
            
            
        }

        void DisplayDepth()
        {
            if (occlusionManager.subsystem.running)
            {
                /*
                if (occlusionManager.TryAcquireEnvironmentDepthCpuImage(out var cpuImage))
                {
                    _depthImage?.Dispose();
                    _depthImage = cpuImage;
                    Debug.Log("acquired cpu image");
                }
                else
                {
                    Debug.Log("could not acquire cpu image");
                }
                */
                
                m_RawImage.texture = occlusionManager.environmentDepthTexture;
                m_RawImage.material.SetTexture("_DepthTex", occlusionManager.environmentDepthTexture);
                m_RawImage.material.SetMatrix("_displayMat", _displayMatrix);
            }
            else
            {
                Debug.Log("occlusion subsystem is NOT running");
            }
        }

        void DispatchBilinear(Texture mask)
        {
            var dim1 = new Vector2Int(mask.width, mask.height);
            var dim2 = new Vector2Int(m_camera.pixelWidth, m_camera.pixelHeight);

            compute.SetTexture(3, "Result", scaledSegmentation);
            compute.SetTexture(3, "Texture", _groundTexture);
            compute.SetFloat("_confidenceThreshold", confidenceThresh);
            compute.SetVector("TexSize", new Vector2(dim2.x, dim2.y));
            compute.SetVector("TexSize2", new Vector2(dim1.x, dim1.y));
            compute.SetMatrix("_displayMatrix", displayMatrix);
            compute.Dispatch(3, dim2.x/8, dim2.y/8, 1);
            //m_maskImage.texture = scaledSegmentation;
            //Debug.Log("successfully dispatched bilinear");
        }

        void DispatchFlood(Texture mask)
        {
            //Debug.Log("started calling flood");
            var dim = new Vector2Int(mask.width, mask.height);
            Graphics.Blit(mask, JFA_Mask);

            compute.SetInt("maxCrawl", 64);
            compute.SetFloat("_jfaSmoothing", JFA_smoothing);
            compute.SetFloat("_jfaExp", JFA_exp);
            compute.SetTexture(0, "Texture", scaledSegmentation);
            compute.SetTexture(0, "Result", JFA_Mask);
            compute.SetVector("TexSize", new Vector2(dim.x, dim.y));
            compute.Dispatch(0, dim.x/8, dim.y/8, 1);
            //Debug.Log("successfully executed init");
            compute.SetTexture(1, "Result", JFA_Mask);
            
            /*
            compute.SetInt("offset", 128);
            
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            */
            
            compute.SetInt("offset", 64);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            
            compute.SetInt("offset", 32);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            
            compute.SetInt("offset", 16);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            //Debug.Log("successfully executed step 1");
            
            compute.SetInt("offset", 8);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            
            compute.SetInt("offset", 4);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            //Debug.Log("successfully executed step 2");
            
            compute.SetInt("offset", 2);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            //Debug.Log("successfully finished JFA");
            
            compute.SetInt("offset", 1);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            
            compute.SetTexture(2, "Texture", JFA_Mask);
            compute.SetTexture(2, "Result", scaledSegmentation);
            compute.Dispatch(2, dim.x/8, dim.y/8, 1);
            //m_maskImage.texture = JFA_Mask;
        }
        
        

        private void UpdateRawImage()
        {
            Debug.Assert(m_RawImage != null, "no raw image");

            // The aspect ratio of the presentation in landscape orientation
            var aspect = Mathf.Max(m_camera.pixelWidth, m_camera.pixelHeight) /
                         (float)Mathf.Min(m_camera.pixelWidth, m_camera.pixelHeight);

            tanFov = Mathf.Tan(Mathf.Deg2Rad * m_camera.fieldOfView / 2.0f);
            Debug.Log($"Updating screen orientation, width: {m_camera.pixelWidth}, height: {m_camera.pixelHeight}");
            // Determine the raw image rectSize preserving the texture aspect ratio, matching the screen orientation,
            // and keeping a minimum dimension size.
            float minDimension = m_camera.pixelWidth;
            float maxDimension = Mathf.Round(minDimension * aspect);
            Vector2 rectSize;
            switch (m_CurrentScreenOrientation)
            {
                case ScreenOrientation.LandscapeRight:
                case ScreenOrientation.LandscapeLeft:
                    rectSize = new Vector2(maxDimension, minDimension);
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                case ScreenOrientation.Portrait:
                default:
                    rectSize = new Vector2(minDimension, maxDimension);
                    break;
            }

            // Update the raw image dimensions and the raw image material parameters.
            m_RawImage.rectTransform.sizeDelta = rectSize;
            m_WireImage.rectTransform.sizeDelta = rectSize;
            m_Stars.rectTransform.sizeDelta = rectSize;
            //m_RawImage.material = m_Material;
        }

        void OnApplicationQuit()
        {
            if(JFA_Mask)
                JFA_Mask.Release();

            if (scaledSegmentation)
                scaledSegmentation.Release();
        }
    }


