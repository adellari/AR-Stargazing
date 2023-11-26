using System.Linq;
using Niantic.Lightship.AR.Semantics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using System.Text;
using UnityEngine.XR.ARFoundation;

namespace Niantic.Lightship.AR.Samples
{
    public class AstroSegmentation : MonoBehaviour
    {

        const string k_DisplayMatrixName = "DisplayMatrix";
        public ARSemanticSegmentationManager segmentationManager;
        private readonly int k_DisplayMatrix = Shader.PropertyToID(k_DisplayMatrixName);

        protected ScreenOrientation m_CurrentScreenOrientation;
        [SerializeField] protected RawImage m_RawImage;
        [SerializeField] protected RawImage m_RawImage2;
        [SerializeField] protected RawImage m_maskImage;
        
        [SerializeField] Material m_Material;
        // The rendering Unity camera
        private Camera m_camera;

        public Texture _texture;
        public Matrix4x4 displayMatrix;
        private bool segmentationReady = false;

        [Header("Compute")] 
        [SerializeField] private RenderTexture JFA_Mask;
        [SerializeField] private ComputeShader compute;


        void Awake()
        {

            // Acquire a reference to the rendering camera
            m_camera = Camera.main;
            segmentationManager = Camera.main.GetComponent<ARSemanticSegmentationManager>();

            segmentationManager.MetadataInitialized += OnSemanticModelReady;
            

            // Get the current screen orientation, and update the raw image UI
            m_CurrentScreenOrientation = Screen.orientation;
        }

        void OnEnable()
        {
            UpdateRawImage();
        }

        private void OnSemanticModelReady(ARSemanticSegmentationModelEventArgs args)
        {
            segmentationReady = true;
        }

        void Update()
        {

            if (m_CurrentScreenOrientation != Screen.orientation)
            {
                m_CurrentScreenOrientation = Screen.orientation;
                UpdateRawImage();
            }

            // Update the image
            var sizeDelta = m_RawImage.rectTransform.sizeDelta;

            var viewport = new XRCameraParams
            {
                screenWidth = (int)(sizeDelta.x), 
                screenHeight = (int)sizeDelta.y,
                screenOrientation = m_CurrentScreenOrientation
            };

            if (segmentationReady)
            {
                _texture = segmentationManager.GetSemanticChannelTexture("artificial_ground", out displayMatrix, viewport);

                m_RawImage.texture = _texture;
                
                m_RawImage.material.SetMatrix(k_DisplayMatrix, displayMatrix);
                m_RawImage.material.SetTexture("_SemanticTex", _texture);
                m_RawImage2.texture = _texture;
                m_RawImage2.material.SetTexture("_SemanticMask", _texture);
                m_RawImage2.material.SetMatrix("_DisplayMatrix", displayMatrix);

                if (!JFA_Mask)
                {
                    var dim = new Vector2Int(_texture.width, _texture.height);
                    JFA_Mask = new RenderTexture(dim.x, dim.y, 0);
                    JFA_Mask.enableRandomWrite = true;
                    JFA_Mask.Create();
                    Debug.Log($"created mask render texture, initial dimensions {_texture.width} by {_texture.height}");
                }
                //Debug.Log("about to dispatch flood");
                DispatchFlood(_texture);

            }
            
        }

        void DispatchFlood(Texture mask)
        {
            //Debug.Log("started calling flood");
            var dim = new Vector2Int(mask.width, mask.height);
            Graphics.Blit(mask, JFA_Mask);
            
            compute.SetTexture(0, "Result", JFA_Mask);
            compute.SetVector("TexSize", new Vector2(dim.x, dim.y));
            compute.Dispatch(0, dim.x/8, dim.y/8, 1);
            //Debug.Log("successfully executed init");
            
            compute.SetInt("offset", 4);
            compute.SetTexture(1, "Result", JFA_Mask);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            //Debug.Log("successfully executed step 1");
            
            compute.SetInt("offset", 3);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            
            compute.SetInt("offset", 2);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            //Debug.Log("successfully executed step 2");
            
            compute.SetInt("offset", 1);
            compute.Dispatch(1, dim.x/8, dim.y/8, 1);
            //Debug.Log("successfully finished JFA");
            
            m_maskImage.texture = JFA_Mask;
        }



        private static void BuildTextureInfo(StringBuilder stringBuilder, string textureName, Texture2D texture)
        {
            stringBuilder.AppendLine($"texture : {textureName}");
            if (texture == null)
            {
                stringBuilder.AppendLine("   <null>");
            }
            else
            {
                stringBuilder.AppendLine($"   format : {texture.format}");
                stringBuilder.AppendLine($"   width  : {texture.width}");
                stringBuilder.AppendLine($"   height : {texture.height}");
                stringBuilder.AppendLine($"   mipmap : {texture.mipmapCount}");
            }
        }
        

        private void UpdateRawImage()
        {
            Debug.Assert(m_RawImage != null, "no raw image");

            // The aspect ratio of the presentation in landscape orientation
            var aspect = Mathf.Max(m_camera.pixelWidth, m_camera.pixelHeight) /
                         (float)Mathf.Min(m_camera.pixelWidth, m_camera.pixelHeight);

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
            m_RawImage.material = m_Material;
        }

        void OnApplicationQuit()
        {
            if(JFA_Mask)
                JFA_Mask.Release();
        }
    }

}
