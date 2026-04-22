using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.Universal
{
    public class ReflectiveDiffuse : MonoBehaviour
    {
        private bool VR = false;
        public int ReflectionTexResolution = 512;
        public float Offset = 0.0f;
        [Range(0, 1)] public float ReflectionAlpha = 0.5f;
        public bool BlurredReflection;
        public LayerMask LayersToReflect = -1;

        private Camera ReflectionCamera;
        private RenderTexture reflectionTexture = null, reflectionTextureRight = null;
        private static bool isRendering = false;
        private Material material;
        private static readonly int reflectionTexString = Shader.PropertyToID("_ReflectionTex");
        private static readonly int reflectionTexRString = Shader.PropertyToID("_ReflectionTexRight");
        private static readonly int reflectionAlphaString = Shader.PropertyToID("_RefAlpha");
        private static readonly string blurString = "BLUR";
        private static readonly string vrString = "VRon";
        private Matrix4x4 reflectionMatrix;
        private Vector4 reflectionPlane;
        private Vector3 position;
        private Vector3 normal;
        private Matrix4x4 projection;
        private Vector4 oblique;
        private Matrix4x4 worldToCameraMatrix;
        private Vector3 clipNormal;
        private Vector4 clipPlane;
        private Vector3 oldPosition;
        Vector3 eulerAngles;


        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += this.RenderObject;
        }


        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= this.RenderObject;
            if (reflectionTexture)
            {
                RemoveObject(reflectionTexture);
                reflectionTexture = null;
            }

            if (reflectionTextureRight)
            {
                RemoveObject(reflectionTextureRight);
                reflectionTextureRight = null;
            }

            if (ReflectionCamera)
            {
                RemoveObject(ReflectionCamera.gameObject);
                ReflectionCamera = null;
            }
        }

        public void Start()
        {
            material = GetComponent<Renderer>().sharedMaterials[0];
            QualitySettings.pixelLightCount = 0;

            var go = new GameObject(GetInstanceID().ToString(), typeof(Camera), typeof(Skybox));
            ReflectionCamera = go.GetComponent<Camera>();
            var URPCamData = go.AddComponent(typeof(UniversalAdditionalCameraData)) as UniversalAdditionalCameraData;
            URPCamData.renderShadows = false;
            URPCamData.requiresColorOption = CameraOverrideOption.Off;
            URPCamData.requiresDepthOption = CameraOverrideOption.Off;
            ReflectionCamera.enabled = false;

            ReflectionCamera.transform.position = transform.position;
            ReflectionCamera.transform.rotation = transform.rotation;

            ReflectionCamera.cullingMask = ~(1 << 4) & LayersToReflect.value;
            ReflectionCamera.cameraType = CameraType.Reflection;

            go.hideFlags = HideFlags.HideAndDontSave;

            if (reflectionTexture)
            {
                RemoveObject(reflectionTexture);
            }

            reflectionTexture = new RenderTexture(ReflectionTexResolution, ReflectionTexResolution, 16)
            {
                isPowerOfTwo = true,
                hideFlags = HideFlags.DontSave
            };

            if (reflectionTextureRight)
            {
                RemoveObject(reflectionTextureRight);
            }

            reflectionTextureRight = new RenderTexture(ReflectionTexResolution, ReflectionTexResolution, 16)
            {
                isPowerOfTwo = true,
                hideFlags = HideFlags.DontSave
            };
        }

        void RenderObject(ScriptableRenderContext context, Camera cam)
        {
            if (isRendering)
            {
                return;
            }

            isRendering = true;
            position = transform.position;
            normal = transform.up;

            ReflectionCamera.clearFlags = cam.clearFlags;
            ReflectionCamera.backgroundColor = cam.backgroundColor;
            ReflectionCamera.farClipPlane = cam.farClipPlane;
            ReflectionCamera.nearClipPlane = cam.nearClipPlane;
            ReflectionCamera.orthographic = cam.orthographic;
            ReflectionCamera.fieldOfView = cam.fieldOfView;
            ReflectionCamera.aspect = cam.aspect;
            ReflectionCamera.orthographicSize = cam.orthographicSize;

            if (cam.clearFlags == CameraClearFlags.Skybox)
            {
                var sky = cam.GetComponent(typeof(Skybox)) as Skybox;
                var CurrentSky = ReflectionCamera.GetComponent(typeof(Skybox)) as Skybox;
                if (!sky || !sky.material)
                {
                    CurrentSky.enabled = false;
                }
                else
                {
                    CurrentSky.enabled = true;
                    CurrentSky.material = sky.material;
                }
            }

            reflectionPlane = new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, position) - Offset);

            reflectionMatrix.m00 = (1F - 2F * reflectionPlane[0] * reflectionPlane[0]);
            reflectionMatrix.m01 = (-2F * reflectionPlane[0] * reflectionPlane[1]);
            reflectionMatrix.m02 = (-2F * reflectionPlane[0] * reflectionPlane[2]);
            reflectionMatrix.m03 = (-2F * reflectionPlane[3] * reflectionPlane[0]);
            reflectionMatrix.m10 = (-2F * reflectionPlane[1] * reflectionPlane[0]);
            reflectionMatrix.m11 = (1F - 2F * reflectionPlane[1] * reflectionPlane[1]);
            reflectionMatrix.m12 = (-2F * reflectionPlane[1] * reflectionPlane[2]);
            reflectionMatrix.m13 = (-2F * reflectionPlane[3] * reflectionPlane[1]);
            reflectionMatrix.m20 = (-2F * reflectionPlane[2] * reflectionPlane[0]);
            reflectionMatrix.m21 = (-2F * reflectionPlane[2] * reflectionPlane[1]);
            reflectionMatrix.m22 = (1F - 2F * reflectionPlane[2] * reflectionPlane[2]);
            reflectionMatrix.m23 = (-2F * reflectionPlane[3] * reflectionPlane[2]);
            reflectionMatrix.m30 = 0F;
            reflectionMatrix.m31 = 0F;
            reflectionMatrix.m32 = 0F;
            reflectionMatrix.m33 = 1F;

            oldPosition = cam.transform.position;
            ReflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflectionMatrix;

            worldToCameraMatrix = ReflectionCamera.worldToCameraMatrix;
            clipNormal = worldToCameraMatrix.MultiplyVector(normal).normalized;
            clipPlane = new Vector4(clipNormal.x, clipNormal.y, clipNormal.z,
                -Vector3.Dot(worldToCameraMatrix.MultiplyPoint(position + normal * Offset), clipNormal));

            if (!VR)
            {
                RenderObjectCamera(cam.projectionMatrix, false);
                material.DisableKeyword(vrString);
                GL.invertCulling = true;
                ReflectionCamera.transform.position = reflectionMatrix.MultiplyPoint(oldPosition);
                eulerAngles = cam.transform.eulerAngles;
                ReflectionCamera.transform.eulerAngles = new Vector3(0, eulerAngles.y, eulerAngles.z);
                UniversalRenderPipeline.RenderSingleCamera(context, ReflectionCamera);
                ReflectionCamera.transform.position = oldPosition;
                GL.invertCulling = false;
                material.SetTexture(reflectionTexString, reflectionTexture);
            }
            else
            {
                RenderObjectCamera(cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left), false);
                material.EnableKeyword(vrString);
                GL.invertCulling = true;
                ReflectionCamera.transform.position = reflectionMatrix.MultiplyPoint(oldPosition);
                eulerAngles = cam.transform.eulerAngles;
                ReflectionCamera.transform.eulerAngles = new Vector3(0, eulerAngles.y, eulerAngles.z);
                UniversalRenderPipeline.RenderSingleCamera(context, ReflectionCamera);
                ReflectionCamera.transform.position = oldPosition;
                GL.invertCulling = false;
                material.SetTexture(reflectionTexString, reflectionTexture);
                RenderObjectCamera(cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right), true);
                GL.invertCulling = true;
                ReflectionCamera.transform.position = reflectionMatrix.MultiplyPoint(oldPosition);
                eulerAngles = cam.transform.eulerAngles;
                ReflectionCamera.transform.eulerAngles = new Vector3(0, eulerAngles.y, eulerAngles.z);
                UniversalRenderPipeline.RenderSingleCamera(context, ReflectionCamera);
                ReflectionCamera.transform.position = oldPosition;
                GL.invertCulling = false;
                material.SetTexture(reflectionTexRString, reflectionTextureRight);
            }

            material.SetFloat(reflectionAlphaString, ReflectionAlpha);

            if (BlurredReflection)
            {
                material.EnableKeyword(blurString);
            }
            else
            {
                material.DisableKeyword(blurString);
            }

            isRendering = false;
        }

        void RemoveObject(Object obj)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        private void RenderObjectCamera(Matrix4x4 projection, bool right)
        {
            oblique = clipPlane * (2.0F / (Vector4.Dot(clipPlane,
                projection.inverse * new Vector4(Sign(clipPlane.x), Sign(clipPlane.y), 1.0f, 1.0f))));
            projection[2] = oblique.x - projection[3];
            projection[6] = oblique.y - projection[7];
            projection[10] = oblique.z - projection[11];
            projection[14] = oblique.w - projection[15];
            ReflectionCamera.projectionMatrix = projection;
            ReflectionCamera.targetTexture = right ? reflectionTextureRight : reflectionTexture;
        }

        private static float Sign(float a)
        {
            return a > 0.0f ? 1.0f : a < 0.0f ? -1.0f : 0.0f;
        }


    }
}
    
    
