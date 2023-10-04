using System.Text;
using Niantic.Lightship.AR.Utilities;
using UnityEngine.XR.ARFoundation.Samples;
using Niantic.Lightship.AR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class AstroSegmentation : MonoBehaviour
    {

        const string k_DisplayMatrixName = "DisplayMatrix";
        public ARSemanticSegmentationManager segmentationManager;
        private readonly int k_DisplayMatrix = Shader.PropertyToID(k_DisplayMatrixName);
        protected readonly StringBuilder m_StringBuilder = new();

        protected ScreenOrientation m_CurrentScreenOrientation;

        [SerializeField]
        [Tooltip("The ARCameraManager which will produce camera frame events.")]
        ARCameraManager m_CameraManager;

        [SerializeField]
        protected RawImage m_RawImage;
        [SerializeField]
        protected RawImage m_RawImage2;

        [SerializeField]
        Material m_Material;

        [SerializeField]
        MeshRenderer skybox;

        [SerializeField]
        Cubemap skycube;

        [SerializeField]
        Text m_ImageInfo;

        // The rendering Unity camera
        private Camera m_camera;

        public Texture _texture;
        public Matrix4x4 displayMatrix;
        

        private bool segmentationReady = false;

        void Awake()
        {
            m_CameraManager = Camera.main.GetComponent<ARCameraManager>();

            // Acquire a reference to the rendering camera
            m_camera = Camera.main;
            segmentationManager = Camera.main.GetComponent<ARSemanticSegmentationManager>();

            segmentationManager.SemanticModelIsReady += OnSemanticModelReady;

            // Get the current screen orientation, and update the raw image UI
            m_CurrentScreenOrientation = Screen.orientation;
        }

        void OnEnable()
        {
            UpdateRawImage();
        }

        private void OnSemanticModelReady(ARSemanticModelReadyEventArgs args)
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
                screenWidth = (int)(sizeDelta.x), //was sizeDelta.x 
                screenHeight = (int)sizeDelta.y,
                screenOrientation = m_CurrentScreenOrientation
            };

            if (segmentationReady)
            {
                // Update the texture with the confidence values of the currently selected channel
                segmentationManager.GetSemanticChannelTexture("artificial_ground", viewport, ref _texture, out displayMatrix);

                m_RawImage.texture = _texture;
                //m_RawImage.material.SetMatrix(k_DisplayMatrix, displayMatrix);
                
                m_RawImage.material.SetMatrix(k_DisplayMatrix, displayMatrix);
                m_RawImage.material.SetTexture("_SemanticTex", _texture);
                m_RawImage2.texture = _texture;
                m_RawImage2.material.SetTexture("_SemanticMask", _texture);
                m_RawImage2.material.SetMatrix("_DisplayMatrix", displayMatrix);
                //m_RawImage.material.SetTexture("_Tex", skycube);

            }
            

            /*
            m_RawImage.material.SetMatrix(k_DisplayMatrix, displayMatrix);

            if (m_RawImage.texture != null)
            {
                // Display some text information about each of the textures.
                var displayTexture = m_RawImage.texture as Texture2D;
                if (displayTexture != null)
                {
                    m_StringBuilder.Clear();
                    //BuildTextureInfo(m_StringBuilder, "env", displayTexture);
                    //LogText(m_StringBuilder.ToString());
                }
            }
            */
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


        private void LogText(string text)
        {
            if (m_ImageInfo != null)
            {
                m_ImageInfo.text = text;
            }
            else
            {
                Debug.Log(text);
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
    }
}

