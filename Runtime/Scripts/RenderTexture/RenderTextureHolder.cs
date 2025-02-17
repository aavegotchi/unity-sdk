using System.Collections.Generic;
using UnityEngine;

public class RenderTextureHolder : MonoBehaviour
{
    [SerializeField] public RenderTexture RenderTexture { get; private set; }
    [SerializeField] public Camera RenderCamera;

    //--------------------------------------------------------------------------------------------------
    public void Initialize(int width, int height)
    {
        RenderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGBHalf);
        RenderCamera.gameObject.SetActive(false);
        RenderCamera.targetTexture = RenderTexture;
    }

    //--------------------------------------------------------------------------------------------------
    public void PerformRender()
    {
        if (RenderCamera != null)
        {
            RenderCamera.Render();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void ReturnToPool()
    {
        RenderTextureManager.Instance.ReleaseHolder(this);
    }
}
