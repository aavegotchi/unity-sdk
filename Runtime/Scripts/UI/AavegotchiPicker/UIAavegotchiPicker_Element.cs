using UnityEngine;
using UnityEngine.UI;

namespace GotchiSDK.UI
{
    public class UIAavegotchiPicker_Element : UIElement_Visibility
    {
        [Header("Aavegotchi")]
        [SerializeField] private RawImage AavegotchiImage;

        public UIAavegotchiPicker ParentPicker;
        public EAavegotchiCombo ConfiguredCombo = EAavegotchiCombo.None;

        public AavegotchiRenderTextureHolder AavegotchiRenderTextureHolder;

        //--------------------------------------------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();
            AavegotchiRenderTextureHolder = RenderTextureManager.Instance.RequestAavegotchiHolder(512, 512);
            AavegotchiImage.texture = AavegotchiRenderTextureHolder.RenderTexture;
        }

        // ------------------------------------------------------
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (AavegotchiRenderTextureHolder != null)
            {
                AavegotchiRenderTextureHolder.ReturnToPool();
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void OnClicked()
        {
            if (ParentPicker != null)
            {
                ParentPicker.GotoItem(ConfiguredCombo);
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void SetupForCombo(EAavegotchiCombo comboID)
        {
            ConfiguredCombo = comboID;

            AavegotchiRenderTextureHolder.AavegotchiRenderer.LoadChampion((int)ConfiguredCombo);
        }
    }
}