using DG.Tweening;
using UnityEngine;

namespace GotchiSDK.UI
{
    public class UIElement_Visibility_CanvasGroup : UIElement_Visibility
    {
        [SerializeField]
        private CanvasGroup CanvasGroup;

        //--------------------------------------------------------------------------------------------------
        protected override void Awake()
        {
            if (CanvasGroup == null)
            {
                CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            }

            base.Awake();
        }

        //--------------------------------------------------------------------------------------------------
        protected override void OnShown()
        {
            base.OnShown();

            CanvasGroup.alpha = 1;
        }

        //--------------------------------------------------------------------------------------------------
        protected override void OnHidden()
        {
            base.OnHidden();

            gameObject.SetActive(false);

            CanvasGroup.alpha = 0;
        }

        //--------------------------------------------------------------------------------------------------
        protected override void SpawnShowAnimations(float currentValue, float duration, Sequence masterSequence)
        {
            base.SpawnShowAnimations(currentValue, duration, masterSequence);

            gameObject.SetActive(true);

            masterSequence.Insert(0.0f, CanvasGroup.DOFade(1.0f, duration));
        }

        //--------------------------------------------------------------------------------------------------
        protected override void SpawnHideAnimations(float currentValue, float duration, Sequence masterSequence)
        {
            base.SpawnHideAnimations(currentValue, duration, masterSequence);

            masterSequence.Insert(0.0f, CanvasGroup.DOFade(0.0f, duration));
        }
    }
}