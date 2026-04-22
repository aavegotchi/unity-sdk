using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflection : MonoBehaviour
{
    private Vector2 Resolution;

    [SerializeField] private Camera ReflectionCamera;
    [SerializeField] private RenderTexture ReflectionRenderTexture;
    [SerializeField] private int ReflectionResloution;

    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    [SerializeField] float updateInterval = 0.1f;
    private float lastUpdateTime;

    private void Start()
    {
        lastCameraPosition = ReflectionCamera.transform.position;
        lastCameraRotation = ReflectionCamera.transform.rotation;
        lastUpdateTime = Time.time;
    }

    private void LateUpdate()
    {
        // Only update if the game is running
        if (!Application.isPlaying)
        {
            return;
        }

        // Check if the interval time has passed
        if (Time.time - lastUpdateTime < updateInterval)
        {
            return;
        }

        // Update the last update time
        lastUpdateTime = Time.time;

        // Check if the reflection camera has moved or rotated
        if (Camera.main.transform.position != lastCameraPosition || Camera.main.transform.rotation != lastCameraRotation)
        {
            ReflectionCamera.fieldOfView = Camera.main.fieldOfView;
            ReflectionCamera.transform.position = new Vector3(Camera.main.transform.position.x, -Camera.main.transform.position.y + transform.position.y, Camera.main.transform.position.z);
            ReflectionCamera.transform.rotation = Quaternion.Euler(-Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, 0f);

            Resolution = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);

            ReflectionRenderTexture.Release();
            ReflectionRenderTexture.width = Mathf.RoundToInt(Resolution.x) * ReflectionResloution / Mathf.RoundToInt(Resolution.y);
            ReflectionRenderTexture.height = ReflectionResloution;

            // Update the last known position and rotation
            lastCameraPosition = ReflectionCamera.transform.position;
            lastCameraRotation = ReflectionCamera.transform.rotation;
        }
    }
}
