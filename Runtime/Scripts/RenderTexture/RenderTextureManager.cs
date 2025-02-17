using System.Collections.Generic;
using UnityEngine;

public class RenderTextureManager : MonoBehaviour
{
    public static RenderTextureManager Instance { get; private set; }

    [SerializeField] private GameObject AavegotchiHolderPrefab;
    private Stack<AavegotchiRenderTextureHolder> AavegotchiPool = new Stack<AavegotchiRenderTextureHolder>();

    private List<RenderTextureHolder> ActiveRenderTextureHolders = new List<RenderTextureHolder>();

    [SerializeField] private Vector2 OffsetPerObject = new Vector2(20, 0);
    private Vector2 CurrentXOffset = Vector2.zero;

    [SerializeField] private float MaxRenderMS = 6.0f;
    private int CurrentRenderIndex = 0;

    [SerializeField] private bool RenderEnabled = true;
    [SerializeField] private bool TimeLimitEnabled = true;

    //--------------------------------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void Update()
    {
        if (!RenderEnabled)
        {
            return;
        }

        float startTime = Time.realtimeSinceStartup;
        float maxTime = MaxRenderMS / 1000.0f;

        // Protection in case ActiveRenderTextureHolders shrank since last frame
        if (CurrentRenderIndex >= ActiveRenderTextureHolders.Count)
        {
            CurrentRenderIndex = 0;
        }

        int loopIndex = CurrentRenderIndex;

        // Loop through ActiveRenderTextureHolders and render textures starting at CurrentRenderIndex to the end
        while (loopIndex < ActiveRenderTextureHolders.Count)
        {
            RenderTextureHolder holder = ActiveRenderTextureHolders[loopIndex];

            float currentTime = Time.realtimeSinceStartup;
            if (TimeLimitEnabled && currentTime - startTime > maxTime)
            {
                // Time limit exceeded, break out of the loop
                CurrentRenderIndex = loopIndex;
                break;
            }

            holder.PerformRender();

            ++loopIndex;
        }

        // Did we reach the end of the list? Perhaps we need to continue to loop from the begginging
        if (loopIndex >= ActiveRenderTextureHolders.Count)
        {
            loopIndex = 0;
            float currentTime = Time.realtimeSinceStartup;

            if (TimeLimitEnabled && currentTime - startTime > maxTime)
            {
                // Time limit exceeded, set the CurrentRenderIndex to 0
                CurrentRenderIndex = loopIndex;
            }
            else
            {
                while (loopIndex < CurrentRenderIndex)
                {
                    RenderTextureHolder holder = ActiveRenderTextureHolders[loopIndex];

                    currentTime = Time.realtimeSinceStartup;
                    if (TimeLimitEnabled && currentTime - startTime > maxTime)
                    {
                        // Time limit exceeded, break out of the loop
                        CurrentRenderIndex = loopIndex;
                        break;
                    }

                    holder.PerformRender();
                    ++loopIndex;
                }
            }
        }
    }


    //--------------------------------------------------------------------------------------------------
    public AavegotchiRenderTextureHolder RequestAavegotchiHolder(int width, int height)
    {
        AavegotchiRenderTextureHolder holder;
        if (AavegotchiPool.Count > 0)
        {
            holder = AavegotchiPool.Pop();
            holder.gameObject.SetActive(true);
        }
        else
        {
            var holderObject = Instantiate(AavegotchiHolderPrefab, transform);
            SetLayerRecursively(holderObject, gameObject.layer);
            holder = holderObject.GetComponent<AavegotchiRenderTextureHolder>();
            holderObject.transform.localPosition = new Vector3(CurrentXOffset.x, CurrentXOffset.y, 0);
            CurrentXOffset += OffsetPerObject;
        }

        holder.Initialize(width, height);

        ActiveRenderTextureHolders.Add(holder);

        return holder;
    }

    //--------------------------------------------------------------------------------------------------
    public void ReleaseHolder(RenderTextureHolder holder)
    {
        holder.RenderCamera.targetTexture = null;
        holder.RenderTexture.Release();
        holder.gameObject.SetActive(false);

        if (ActiveRenderTextureHolders.Contains(holder))
        {
            ActiveRenderTextureHolders.Remove(holder);
        }

        if (holder is AavegotchiRenderTextureHolder)
        {
            AavegotchiPool.Push(holder as AavegotchiRenderTextureHolder);
        }
        else
        {
            Destroy(holder.gameObject);
        }
    }

    // --------------------------------------------------------------------------------------------------
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
