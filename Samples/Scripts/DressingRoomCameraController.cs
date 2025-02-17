using UnityEngine;

public class DressingRoomCameraController : MonoBehaviour
{
    [SerializeField] private Vector3 MaxZoomedOutPos;
    [SerializeField] private Vector3 MaxZoomedInPos;

    [SerializeField] private float ScollFactor = 0.1f;
    [SerializeField] private float StartZoomFactor = 0.0f; // 0.0f = max zoomed out, 1.0f = max zoomed in
    private float CurrentZoom;

    //--------------------------------------------------------------------------------------------------
    void Start()
    {
        UpdateCameraPosFromZoomFactor(StartZoomFactor);
    }

    //--------------------------------------------------------------------------------------------------
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
            UpdateCameraPosFromZoomFactor(CurrentZoom + ScollFactor * scroll);
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void UpdateCameraPosFromZoomFactor(float newFactor)
    {
        CurrentZoom = Mathf.Clamp(newFactor, 0.0f, 1.0f);

        transform.localPosition = Vector3.Lerp(MaxZoomedOutPos, MaxZoomedInPos, CurrentZoom);
    }
}
