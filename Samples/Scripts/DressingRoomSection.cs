using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DressingRoomSection : MonoBehaviour
{
    [SerializeField] public GameObject ContentRoot;
    [SerializeField] public RectTransform CollapseBtn;
    [SerializeField] public TMPro.TextMeshProUGUI Label;
    [SerializeField] public string LabelName;
    [SerializeField] public bool ActivatesItem = true;
    [SerializeField] public Transform HiddenContentRoot;

    private bool IsVisible = false;

    public int CurrentIndex = -1;

    public UnityEvent<int> OnItemSelected = new UnityEvent<int>();
    public UnityEvent<DressingRoomSection> OnSectionOpened = new UnityEvent<DressingRoomSection>();

    [SerializeField] public List<DressingRoomSectionItem> SectionItems = new List<DressingRoomSectionItem>();

    //--------------------------------------------------------------------------------------------------
    public void Awake()
    {
        SetContentVisible(false);
    }

    //--------------------------------------------------------------------------------------------------
    public void Start()
    {
        for (int i = 0; i < SectionItems.Count; i++)
        {
            var currentIndex = i;
            var currentItem = SectionItems[i];
            if (currentItem != null && currentItem.Btn != null)
            {
                currentItem.Btn.onClick.AddListener(() =>
                {
                    SetCurrentItem(currentIndex);
                    OnItemSelected.Invoke(currentItem.ID);
                });
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void AddDressingRoomSectionItem(DressingRoomSectionItem item, bool isItemEnabled)
    {
        var itemIndex = SectionItems.Count;
        SectionItems.Add(item);
        item.transform.SetParent(ContentRoot.transform, false);
        if (isItemEnabled)
        {
            item.Btn.onClick.AddListener(() =>
            {
                SetCurrentItem(itemIndex);
                OnItemSelected.Invoke(item.ID);
            });
        }
        else
        {
            item.CanvasGroup.alpha = 0.4f;
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void SetCurrentItem(int index, bool force = false)
    {
        if (index != CurrentIndex || force)
        {
            if (ActivatesItem)
            {
                var oldItem = SectionItems[CurrentIndex];
                oldItem.SelectedHighlight.gameObject.SetActive(false);
            }

            CurrentIndex = index;

            if (ActivatesItem)
            {
                var newItem = SectionItems[CurrentIndex];
                newItem.SelectedHighlight.gameObject.SetActive(true);

                Label.text = $"{LabelName} - {newItem.SelectedText}";
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    public DressingRoomSectionItem GetCurrentItem()
    {
        return SectionItems[CurrentIndex];
    }

    //--------------------------------------------------------------------------------------------------
    public void OnDestroy()
    {
        foreach (var item in SectionItems)
        {
            if (item.Btn != null)
            {
                item.Btn.onClick.RemoveAllListeners();
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void ToggleCollapse()
    {
        SetContentVisible(!IsVisible);
    }

    //--------------------------------------------------------------------------------------------------
    public void SetContentVisible(bool visible)
    {
        IsVisible = visible;

        if (visible)
        {
            ContentRoot.transform.SetParent(transform, false);
        }
        else
        {
            ContentRoot.transform.SetParent(HiddenContentRoot, false);
        }

        CollapseBtn.localScale = new Vector3(1, IsVisible ? -1 : 1, 1);

        RectTransformDimensionsUpdate_Rec(transform);

        if (visible)
        {
            OnSectionOpened?.Invoke(this);
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void RectTransformDimensionsUpdate_Rec(Transform currentTransform)
    {
        //currentTransform.gameObject.SetActive(false);
        //currentTransform.gameObject.SetActive(true);

        if (currentTransform.TryGetComponent<VerticalLayoutGroup>(out var verticalLayoutGroup))
        {
            verticalLayoutGroup.enabled = false;
            verticalLayoutGroup.enabled = true;
        }

        Canvas.ForceUpdateCanvases();

        if (currentTransform.parent != null)
        {
            RectTransformDimensionsUpdate_Rec(currentTransform.parent);
        }
    }
}
