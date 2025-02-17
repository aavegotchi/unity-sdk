using DG.Tweening;
using GG.Gameplay.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;
using static AavegotchiWearable_Database;

public class DressingRoomManager : MonoBehaviour
{
    [SerializeField] private Transform ShowcaseRoot;
    [SerializeField] public Aavegotchi_Data ActiveData;
    [SerializeField] public Aavegotchi_Base AavegotchiBase;

    [SerializeField] public DressingRoomSectionItem DressingRoomItemPrefab;

    [Header("Aavegotchi Trait Section")]
    [SerializeField] public DressingRoomSection CollateralSection;

    [SerializeField] public DressingRoomSection EyeShapes;
    [SerializeField] public Image MythicalHighEyeImage;
    [SerializeField] public List<Sprite> MythicalHighEyeSprites = new List<Sprite>();

    [SerializeField] public DressingRoomSection EyeColors;
    [SerializeField] public Image CommonEyeColorImage;

    [Header("Wearables")]
    [SerializeField] public AavegotchiWearable_Database WearableDatabase;
    [SerializeField] public DressingRoomSection HeadWearables;
    [SerializeField] public DressingRoomSection BodyWearables;
    [SerializeField] public DressingRoomSection FaceWearables;
    [SerializeField] public DressingRoomSection EyeWearables;
    [SerializeField] public DressingRoomSection LHandWearables;
    [SerializeField] public DressingRoomSection RHandWearables;
    [SerializeField] public DressingRoomSection PetWearables;

    [Header("Scene")]
    [SerializeField] public DressingRoomSection SceneBackgrounds;
    [SerializeField] public GameObject ForestSceneBG;
    [SerializeField] public List<Color> CameraBGColors = new List<Color>();
    [SerializeField] public Camera MainCamera;

    [Header("Animations")]
    [SerializeField] private Animator GotchiAnimator;
    [SerializeField] public DressingRoomSection AnimStates;
    [SerializeField] public List<string> AnimStateAnimatorKeys = new List<string>();
    private Sequence MoveAngleTweener = null;
    private int _LastAppliedAnimState = -1;
    [SerializeField] public DressingRoomSection AnimTriggers;
    [SerializeField] public List<string> AnimTriggerAnimatorKeys = new List<string>();
    [SerializeField] public List<KeyCode> AnimTriggerKeyCodes = new List<KeyCode>();
    [SerializeField] public List<KeyCode> AlternateAnimTriggerKeyCodes = new List<KeyCode>();

    [Header("Auto Rotator")]
    [SerializeField] private GameObject AutoRotatorRoot;
    [SerializeField] private Button AutoRotatorBtn;
    [SerializeField] private TMP_InputField AutoRotatorSpeed;
    [SerializeField] private TextMeshProUGUI AutoRotatorStatusText;
    private Sequence AutoRotatorSequence = null;
    private bool AutoRotateOn = false;
    [SerializeField] private AdvancedButtonController ManualDragBtnController;
    [SerializeField] private float MouseXToEularRotatio = 2.0f;
    private bool IsDragging = false;
    private float LastMouseLocationX = 0.0f;

    [Header("UI Toggle")]
    [SerializeField] public Button ToggleUIBtn;
    [SerializeField] public RectTransform ToggleUIBtnIcon;
    [SerializeField] public CanvasGroup LeftPaneCG;
    [SerializeField] public CanvasGroup RightPaneCG;
    private bool UIIsVisible = true;

    private Color LastSetColor = Color.black;

    IList<IResourceLocation> WearableAssetList = null;

    //--------------------------------------------------------------------------------------------------
    public void Start()
    {
        // Rely on this classes AavegotchiData to drive the true Gotchi experience we want
        AavegotchiBase.UpdateForData(ActiveData);

        SetupTraitsSection();

        SetupSceneSection();

        SetupWearableSections();

        SetupAnimSection();

        SetupAutoRotator();

        SetupToggleUI();

        SceneBackgrounds.RectTransformDimensionsUpdate_Rec(SceneBackgrounds.transform);
        Canvas.ForceUpdateCanvases();
    }

    //--------------------------------------------------------------------------------------------------
    public void OnDestroy()
    {
        CollateralSection.OnItemSelected.RemoveAllListeners();
        EyeShapes.OnItemSelected.RemoveAllListeners();
        EyeColors.OnItemSelected.RemoveAllListeners();

        HeadWearables.OnItemSelected.RemoveAllListeners();
        BodyWearables.OnItemSelected.RemoveAllListeners();
        FaceWearables.OnItemSelected.RemoveAllListeners();
        EyeWearables.OnItemSelected.RemoveAllListeners();   
        LHandWearables.OnItemSelected.RemoveAllListeners();
        RHandWearables.OnItemSelected.RemoveAllListeners();
        PetWearables.OnItemSelected.RemoveAllListeners();

        SceneBackgrounds.OnItemSelected.RemoveAllListeners();

        HeadWearables.OnSectionOpened.RemoveAllListeners();
        BodyWearables.OnSectionOpened.RemoveAllListeners();
        FaceWearables.OnSectionOpened.RemoveAllListeners();
        EyeWearables.OnSectionOpened.RemoveAllListeners();
        LHandWearables.OnSectionOpened.RemoveAllListeners();
        RHandWearables.OnSectionOpened.RemoveAllListeners();
        PetWearables.OnSectionOpened.RemoveAllListeners();

        AnimStates.OnItemSelected.RemoveAllListeners();
        AnimStates.OnSectionOpened.RemoveAllListeners();

        AnimTriggers.OnItemSelected.RemoveAllListeners();
        AnimTriggers.OnSectionOpened.RemoveAllListeners();

        ToggleUIBtn.onClick.RemoveAllListeners();

        if (MoveAngleTweener != null)
        {
            MoveAngleTweener.Kill();
            MoveAngleTweener = null;
        }

        AutoRotatorBtn.onClick.RemoveAllListeners();

        if (AutoRotatorSequence != null)
        {
            AutoRotatorSequence.Kill();
            AutoRotatorSequence = null;
        }
    }

    //--------------------------------------------------------------------------------------------------
    void Update()
    {
        UpdateAnimTriggerHotkeys();
    }

    //--------------------------------------------------------------------------------------------------
    private void SetupTraitsSection()
    {
        // Set the eye color for common to be with the Primary Color
        CommonEyeColorImage.color = AavegotchiBase.Collaterals.GetData(ActiveData.CollateralType).PrimaryColor;

        // Update the icon of the mythical high eye shape
        UpdateMythicalHighEyeShape();

        // Start with ETH
        CollateralSection.SetCurrentItem(0, force: true);
        // Whenever a user clicks on a DessingRoomSectionItem, update with its ID
        CollateralSection.OnItemSelected.AddListener((id) =>
        {
            ActiveData.CollateralType = (ECollateral)id;
            UpdateEyes(EyeShapes.GetCurrentItem().ID, ActiveData.CollateralType);
            AavegotchiBase.UpdateForData(ActiveData);

            UpdateMythicalHighEyeShape();
            CommonEyeColorImage.color = AavegotchiBase.Collaterals.GetData(ActiveData.CollateralType).PrimaryColor;
        });

        // Same for Eye Shapes. -1 is saved for Mythical since that one changes with collateral
        EyeShapes.SetCurrentItem(1, force: true);
        EyeShapes.OnItemSelected.AddListener((id) =>
        {
            UpdateEyes(id, (ECollateral)CollateralSection.GetCurrentItem().ID);
            AavegotchiBase.UpdateForData(ActiveData);
        });

        // Same for Eye Colors
        EyeColors.SetCurrentItem(0, true);
        EyeColors.OnItemSelected.AddListener((id) =>
        {
            ActiveData.EyeColor = (EEyeColor)id;
            AavegotchiBase.UpdateForData(ActiveData);
        });
    }

    //--------------------------------------------------------------------------------------------------
    private void UpdateEyes(int shape, ECollateral collateral)
    {
        if (shape == -1)
        {
            // Assumes the ECollateral values map to EEyeShape values 1:1 (true at the time of writing this code)
            ActiveData.EyeShape = (EEyeShape)collateral;
        }
        else
        {
            ActiveData.EyeShape = (EEyeShape)shape;
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void UpdateMythicalHighEyeShape()
    {
        MythicalHighEyeImage.sprite = MythicalHighEyeSprites[CollateralSection.GetCurrentItem().ID];
    }

    //--------------------------------------------------------------------------------------------------
    private void SetupSceneSection()
    {
        SceneBackgrounds.SetCurrentItem(0, force: true);

        SceneBackgrounds.OnItemSelected.AddListener((id) =>
        {
            ForestSceneBG.SetActive(id == -1);

            if (id >= 0)
            {
                MainCamera.backgroundColor = CameraBGColors[id];
            }
        });
    }

    //--------------------------------------------------------------------------------------------------
    private void SetupWearableSections()
    {
        if (WearableAssetList == null)
        {
            Addressables.LoadResourceLocationsAsync("Wearable").Completed += (task) =>
            {
                if (task.Status == AsyncOperationStatus.Succeeded)
                {
                    if (task.Result != null)
                    {
                        WearableAssetList = task.Result;

                        WearablesSectionReadyForSetup();
                    }
                }
            };
        }
        else
        {
            WearablesSectionReadyForSetup();
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void WearablesSectionReadyForSetup()
    {
        // Head wearables
        {
            // This function creates all the buttons for all the head wearables and ensures
            // they are configured correctly
            SetupWearableSection(WearableDatabase.HeadWearables, HeadWearables);

            // Whenever the selected wearable changes, ensure that the aavegotchi data is updated for it
            HeadWearables.OnItemSelected.AddListener((id) =>
            {
                ActiveData.Head_WearableID = id;
                AavegotchiBase.UpdateForData(ActiveData, () =>
                {
                });
            });

            // Start with no headwear
            ActiveData.Head_WearableID = 0;
            HeadWearables.SetCurrentItem(0, true);

            // Auto collapsed other wearable sections when this one is opened
            HeadWearables.OnSectionOpened.AddListener((section) =>
            {
                //HeadWearables.SetContentVisible(false);
                BodyWearables.SetContentVisible(false);
                FaceWearables.SetContentVisible(false);
                EyeWearables.SetContentVisible(false);
                LHandWearables.SetContentVisible(false);
                RHandWearables.SetContentVisible(false);
                PetWearables.SetContentVisible(false);
            });
        }

        // Body wearables
        {
            SetupWearableSection(WearableDatabase.BodyWearables, BodyWearables);
            BodyWearables.OnItemSelected.AddListener((id) =>
            {
                ActiveData.Body_WearableID = id;
                AavegotchiBase.UpdateForData(ActiveData, () =>
                {
                });
            });
            // Start with no headwear
            ActiveData.Body_WearableID = 0;
            BodyWearables.SetCurrentItem(0, true);
            BodyWearables.OnSectionOpened.AddListener((section) =>
            {
                HeadWearables.SetContentVisible(false);
                //BodyWearables.SetContentVisible(false);
                FaceWearables.SetContentVisible(false);
                EyeWearables.SetContentVisible(false);
                LHandWearables.SetContentVisible(false);
                RHandWearables.SetContentVisible(false);
                PetWearables.SetContentVisible(false);
            });
        }

        // Face wearables
        {
            SetupWearableSection(WearableDatabase.FaceWearables, FaceWearables);
            FaceWearables.OnItemSelected.AddListener((id) =>
            {
                ActiveData.Face_WearableID = id;
                AavegotchiBase.UpdateForData(ActiveData, () =>
                {
                });
            });
            // Start with no headwear
            ActiveData.Face_WearableID = 0;
            FaceWearables.SetCurrentItem(0, true);
            FaceWearables.OnSectionOpened.AddListener((section) =>
            {
                HeadWearables.SetContentVisible(false);
                BodyWearables.SetContentVisible(false);
                //FaceWearables.SetContentVisible(false);
                EyeWearables.SetContentVisible(false);
                LHandWearables.SetContentVisible(false);
                RHandWearables.SetContentVisible(false);
                PetWearables.SetContentVisible(false);
            });
        }

        // Eye wearables
        {
            SetupWearableSection(WearableDatabase.EyeWearables, EyeWearables);
            EyeWearables.OnItemSelected.AddListener((id) =>
            {
                ActiveData.Eyes_WearableID = id;
                AavegotchiBase.UpdateForData(ActiveData, () =>
                {
                });
            });
            // Start with no headwear
            ActiveData.Eyes_WearableID = 0;
            EyeWearables.SetCurrentItem(0, true);
            EyeWearables.OnSectionOpened.AddListener((section) =>
            {
                HeadWearables.SetContentVisible(false);
                BodyWearables.SetContentVisible(false);
                FaceWearables.SetContentVisible(false);
                //EyeWearables.SetContentVisible(false);
                LHandWearables.SetContentVisible(false);
                RHandWearables.SetContentVisible(false);
                PetWearables.SetContentVisible(false);
            });
        }

        // Left Hand wearables
        {
            SetupWearableSection(WearableDatabase.HandWearables, LHandWearables);
            LHandWearables.OnItemSelected.AddListener((id) =>
            {
                ActiveData.HandLeft_WearableID = id;
                AavegotchiBase.UpdateForData(ActiveData, () =>
                {
                });
            });
            // Start with no headwear
            ActiveData.HandLeft_WearableID = 0;
            LHandWearables.SetCurrentItem(0, true);
            LHandWearables.OnSectionOpened.AddListener((section) =>
            {
                HeadWearables.SetContentVisible(false);
                BodyWearables.SetContentVisible(false);
                FaceWearables.SetContentVisible(false);
                EyeWearables.SetContentVisible(false);
                //LHandWearables.SetContentVisible(false);
                RHandWearables.SetContentVisible(false);
                PetWearables.SetContentVisible(false);
            });
        }

        // Right Hand wearables
        {
            SetupWearableSection(WearableDatabase.HandWearables, RHandWearables);
            RHandWearables.OnItemSelected.AddListener((id) =>
            {
                ActiveData.HandRight_WearableID = id;
                AavegotchiBase.UpdateForData(ActiveData, () =>
                {
                });
            });
            // Start with no headwear
            ActiveData.HandRight_WearableID = 0;
            RHandWearables.SetCurrentItem(0, true);
            RHandWearables.OnSectionOpened.AddListener((section) =>
            {
                HeadWearables.SetContentVisible(false);
                BodyWearables.SetContentVisible(false);
                FaceWearables.SetContentVisible(false);
                EyeWearables.SetContentVisible(false);
                LHandWearables.SetContentVisible(false);
                //RHandWearables.SetContentVisible(false);
                PetWearables.SetContentVisible(false);
            });
        }

        // pet wearables
        {
            SetupWearableSection(WearableDatabase.PetWearables, PetWearables);
            PetWearables.OnItemSelected.AddListener((id) =>
            {
                ActiveData.Pet_WearableID = id;
                AavegotchiBase.UpdateForData(ActiveData, () =>
                {
                });
            });
            // Start with no headwear
            ActiveData.Pet_WearableID = 0;
            PetWearables.SetCurrentItem(0, true);
            PetWearables.OnSectionOpened.AddListener((section) =>
            {
                HeadWearables.SetContentVisible(false);
                BodyWearables.SetContentVisible(false);
                FaceWearables.SetContentVisible(false);
                EyeWearables.SetContentVisible(false);
                LHandWearables.SetContentVisible(false);
                RHandWearables.SetContentVisible(false);
                //PetWearables.SetContentVisible(false);
            });
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void SetupWearableSection(List<AavegotchiWearable_DatabaseEntry> wearableData, DressingRoomSection section)
    {
        var noItem = Instantiate(DressingRoomItemPrefab, section.transform);
        noItem.ID = 0;
        noItem.SelectedText = "Nothing";
        noItem.Icon.color = Color.clear;
        section.AddDressingRoomSectionItem(noItem, true);

        foreach (var wearable in wearableData)
        {
            var item = Instantiate(DressingRoomItemPrefab, section.transform);
            item.ID = wearable.WearableID;
            item.SelectedText = $"{wearable.WearableID} - {wearable.WearableName}";
            item.Icon.sprite = wearable.WearableIcon;
            bool wearableReady = false;
            string addressablesKey = $"Wearable_Mesh_{wearable.WearableID}";
            foreach (var resourceLocation in WearableAssetList)
            {
                if (resourceLocation.PrimaryKey.Equals(addressablesKey))
                {
                    wearableReady = true;
                    break;
                }
            }

            section.AddDressingRoomSectionItem(item, wearableReady);
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void SetupAnimSection()
    {
        AnimStates.SetCurrentItem(0, force: true);
        AnimStates.OnItemSelected.AddListener((id) =>
        {
            if (_LastAppliedAnimState != -1 && !string.IsNullOrEmpty(AnimStateAnimatorKeys[_LastAppliedAnimState]))
            {
                GotchiAnimator.SetBool(AnimStateAnimatorKeys[_LastAppliedAnimState], false);
            }

            _LastAppliedAnimState = id;

            if (!string.IsNullOrEmpty(AnimStateAnimatorKeys[_LastAppliedAnimState]))
            {
                GotchiAnimator.SetBool(AnimStateAnimatorKeys[_LastAppliedAnimState], true);

                if (_LastAppliedAnimState == 2) // Moving forward
                {
                    if (MoveAngleTweener != null)
                    {
                        MoveAngleTweener.Kill();
                    }
                    MoveAngleTweener = DOTween.Sequence();
                    MoveAngleTweener.Insert(0.0f, DOTween.To(() => GotchiAnimator.GetFloat("DirectionZ"),
                                                             (x) => GotchiAnimator.SetFloat("DirectionZ", x),
                                                             0.7f, 0.4f));
                    MoveAngleTweener.Insert(0.0f, DOTween.To(() => GotchiAnimator.GetFloat("DirectionX"),
                                                             (x) => GotchiAnimator.SetFloat("DirectionX", x),
                                                             0.0f, 0.4f));
                }
                else if (_LastAppliedAnimState == 3) // Moving backward
                {
                    if (MoveAngleTweener != null)
                    {
                        MoveAngleTweener.Kill();
                    }
                    MoveAngleTweener = DOTween.Sequence();
                    MoveAngleTweener.Insert(0.0f, DOTween.To(() => GotchiAnimator.GetFloat("DirectionZ"),
                                                             (x) => GotchiAnimator.SetFloat("DirectionZ", x),
                                                             -0.7f, 0.4f));
                    MoveAngleTweener.Insert(0.0f, DOTween.To(() => GotchiAnimator.GetFloat("DirectionX"),
                                                             (x) => GotchiAnimator.SetFloat("DirectionX", x),
                                                             0.0f, 0.4f));
                }
                else if (_LastAppliedAnimState == 4) // Moving Left
                {
                    if (MoveAngleTweener != null)
                    {
                        MoveAngleTweener.Kill();
                    }
                    MoveAngleTweener = DOTween.Sequence();
                    MoveAngleTweener.Insert(0.0f, DOTween.To(() => GotchiAnimator.GetFloat("DirectionZ"),
                                                             (x) => GotchiAnimator.SetFloat("DirectionZ", x),
                                                             0.0f, 0.4f));
                    MoveAngleTweener.Insert(0.0f, DOTween.To(() => GotchiAnimator.GetFloat("DirectionX"),
                                                             (x) => GotchiAnimator.SetFloat("DirectionX", x),
                                                             -0.7f, 0.4f));
                }
                else if (_LastAppliedAnimState == 5) // Moving Right
                {
                    if (MoveAngleTweener != null)
                    {
                        MoveAngleTweener.Kill();
                    }
                    MoveAngleTweener = DOTween.Sequence();
                    MoveAngleTweener.Insert(0.0f, DOTween.To(() => GotchiAnimator.GetFloat("DirectionZ"),
                                                             (x) => GotchiAnimator.SetFloat("DirectionZ", x),
                                                             0.0f, 0.4f));
                    MoveAngleTweener.Insert(0.0f, DOTween.To(() => GotchiAnimator.GetFloat("DirectionX"),
                                                             (x) => GotchiAnimator.SetFloat("DirectionX", x),
                                                             0.7f, 0.4f));
                }
            }
        });

        AnimStates.OnSectionOpened.AddListener((section) =>
        {
            //AnimStates.SetContentVisible(false);
            AnimTriggers.SetContentVisible(false);
        });

        AnimTriggers.OnItemSelected.AddListener((id) =>
        {
            if (!string.IsNullOrEmpty(AnimTriggerAnimatorKeys[id]))
            {
                GotchiAnimator.SetTrigger(AnimTriggerAnimatorKeys[id]);
            }
        });

        AnimTriggers.OnSectionOpened.AddListener((section) =>
        {
            AnimStates.SetContentVisible(false);
            //AnimTriggers.SetContentVisible(false);
        });
    }

    //--------------------------------------------------------------------------------------------------
    private void UpdateAnimTriggerHotkeys()
    {
        for (int i = 0; i < AnimTriggerKeyCodes.Count && i < AnimTriggerAnimatorKeys.Count && i < AlternateAnimTriggerKeyCodes.Count; ++i)
        {
            if (Input.GetKeyUp(AnimTriggerKeyCodes[i]) || Input.GetKeyUp(AlternateAnimTriggerKeyCodes[i]))
            {
                GotchiAnimator.SetTrigger(AnimTriggerAnimatorKeys[i]);
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void SetupAutoRotator()
    {
        AutoRotatorBtn.onClick.AddListener(() =>
        {
            float rotationSpeed = 1.0f;
            float.TryParse(AutoRotatorSpeed.text, out rotationSpeed);

            AutoRotateOn = !AutoRotateOn;

            AutoRotatorStatusText.text = AutoRotateOn ? "Auto Rotator: ON" : "Auto Rotator: OFF";

            if (AutoRotatorSequence != null)
            {
                AutoRotatorSequence.Kill();
                AutoRotatorSequence = null;
            }

            if (AutoRotateOn && !IsDragging && rotationSpeed > 0.0f)
            {
                AutoRotatorSequence = DOTween.Sequence();
                AutoRotatorSequence.SetLoops(-1);

                float duration = 4.0f / rotationSpeed;

                float currentEular = ShowcaseRoot.rotation.eulerAngles.y;
                ShowcaseRoot.rotation = Quaternion.identity;
                AutoRotatorSequence.Insert(0.0f, ShowcaseRoot.DORotate(new Vector3(0f, 360f, 0f), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));

                if (currentEular > 0.0f)
                {
                    AutoRotatorSequence.Goto(currentEular / 360.0f * duration);
                    AutoRotatorSequence.Play();
                }
            }
        });

        AutoRotatorSpeed.onValueChanged.AddListener((value) =>
        {
            float rotationSpeed = 1.0f;
            float.TryParse(AutoRotatorSpeed.text, out rotationSpeed);

            if (AutoRotatorSequence != null)
            {
                AutoRotatorSequence.Kill();
                AutoRotatorSequence = null;
            }

            if (rotationSpeed > 0.0f && AutoRotateOn && !IsDragging)
            {
                AutoRotatorSequence = DOTween.Sequence();
                AutoRotatorSequence.SetLoops(-1);

                float duration = 4.0f / rotationSpeed;

                float currentEular = ShowcaseRoot.rotation.eulerAngles.y;
                ShowcaseRoot.rotation = Quaternion.identity;
                AutoRotatorSequence.Insert(0.0f, ShowcaseRoot.DORotate(new Vector3(0f, 360f, 0f), 4.0f / rotationSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear));

                if (currentEular > 0.0f)
                {
                    AutoRotatorSequence.Goto(currentEular / 360.0f * duration);
                    AutoRotatorSequence.Play();
                }
            }
        });
    }

    //--------------------------------------------------------------------------------------------------
    private void SetupToggleUI()
    {
        ToggleUIBtn.onClick.AddListener(() =>
        {
            UIIsVisible = !UIIsVisible;

            LeftPaneCG.gameObject.SetActive(UIIsVisible);
            LeftPaneCG.alpha = UIIsVisible ? 1.0f : 0.0f;

            RightPaneCG.gameObject.SetActive(UIIsVisible);
            RightPaneCG.alpha = UIIsVisible ? 1.0f : 0.0f;

            AutoRotatorRoot.SetActive(UIIsVisible);

            ToggleUIBtnIcon.localEulerAngles = new Vector3(0, 0, UIIsVisible ? 90 : -90);
        });
    }

    //--------------------------------------------------------------------------------------------------
    public void OnMouseDragStarted()
    {
        IsDragging = true;
        LastMouseLocationX = ManualDragBtnController.CurrentDragLocation.x;

        if (AutoRotatorSequence != null)
        {
            AutoRotatorSequence.Kill();
            AutoRotatorSequence = null;
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void OnMouseDragUpdated()
    {
        var deltaX = ManualDragBtnController.CurrentDragLocation.x - LastMouseLocationX;
        LastMouseLocationX = ManualDragBtnController.CurrentDragLocation.x;
        ShowcaseRoot.Rotate(new Vector3(0, deltaX * MouseXToEularRotatio, 0));
    }

    //--------------------------------------------------------------------------------------------------
    public void OnMouseDragEnd()
    {
        var deltaX = ManualDragBtnController.CurrentDragLocation.x - LastMouseLocationX;
        LastMouseLocationX = ManualDragBtnController.CurrentDragLocation.x;
        ShowcaseRoot.Rotate(new Vector3(0, deltaX * MouseXToEularRotatio, 0));
        IsDragging = false;

        if (AutoRotateOn)
        {
            float rotationSpeed = 1.0f;
            float.TryParse(AutoRotatorSpeed.text, out rotationSpeed);

            AutoRotatorSequence = DOTween.Sequence();
            AutoRotatorSequence.SetLoops(-1);

            float duration = 4.0f / rotationSpeed;

            float currentEular = ShowcaseRoot.rotation.eulerAngles.y;
            ShowcaseRoot.rotation = Quaternion.identity;
            AutoRotatorSequence.Insert(0.0f, ShowcaseRoot.DORotate(new Vector3(0f, 360f, 0f), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));

            if (currentEular > 0.0f)
            {
                AutoRotatorSequence.Goto(currentEular / 360.0f * duration);
                AutoRotatorSequence.Play();
            }
        }
    }
}
