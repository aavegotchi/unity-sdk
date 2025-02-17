using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GotchiSDK.UI
{
    public class UIAavegotchiPicker : UIElement_Visibility_CanvasGroup
    {
        [SerializeField] private UIAavegotchiPicker_Element AavegotchiElementPrefab;
        [SerializeField] private Transform ElementParent;

        [SerializeField] private float NumVisibleElements = 5;
        [SerializeField] private float EdgeScale = 0.6f;
        [SerializeField] private float Padding = 35.0f;

        private List<UIAavegotchiPicker_Element> AavegotchiPickerList = new List<UIAavegotchiPicker_Element>();

        public EAavegotchiCombo CurrentItem;
        private EAavegotchiCombo ItemBeforeTween;
        private float CurrentPos;

        private float ElementWidth;

        Sequence ActiveSequence = null;

        //--------------------------------------------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();

            CreateAllElements();
        }

        //--------------------------------------------------------------------------------------------------
        public void SetCurrentItem(EAavegotchiCombo item)
        {
            CurrentItem = item;
            CurrentPos = (int)item;

            UpdatePositions();
        }

        //--------------------------------------------------------------------------------------------------
        public void GotoNext()
        {
            int newIndex = (int)CurrentItem + 1;
            var numItems = Enum.GetValues(typeof(EAavegotchiCombo)).Length;
            if (newIndex >= numItems)
            {
                newIndex -= numItems;
            }

            GotoItem((EAavegotchiCombo)newIndex);
        }

        //--------------------------------------------------------------------------------------------------
        public void GotoPrev()
        {
            int newIndex = (int)CurrentItem - 1;
            if (newIndex < 0)
            {
                newIndex += Enum.GetValues(typeof(EAavegotchiCombo)).Length;
            }

            GotoItem((EAavegotchiCombo)newIndex);
        }

        //--------------------------------------------------------------------------------------------------
        public void GotoItem(EAavegotchiCombo item)
        {
            if (ActiveSequence != null)
            {
                ActiveSequence.Kill();
                ActiveSequence = null;
            }

            ItemBeforeTween = CurrentItem;

            // Calculate the shortest distance to the target item treating it as a circle
            int comboCount = Enum.GetValues(typeof(EAavegotchiCombo)).Length;
            int currentIndex = (int)CurrentItem;
            int targetIndex = (int)item;
            int clockwiseDistance = (targetIndex - currentIndex + comboCount) % comboCount;
            int counterClockwiseDistance = (currentIndex - targetIndex + comboCount) % comboCount;

            bool useClockwise = clockwiseDistance <= counterClockwiseDistance;
            int shortestDistance = useClockwise ? clockwiseDistance : -counterClockwiseDistance;

            // Calculate the tween duration based on the shortest distance
            float tweenDuration = 0.5f * Mathf.Abs(shortestDistance);

            ActiveSequence = DOTween.Sequence();
            ActiveSequence.Append(DOTween.To(() => CurrentPos,
                                             x =>
                                             {
                                                 if (x < 0.0f)
                                                 {
                                                     CurrentItem = (EAavegotchiCombo)(Mathf.RoundToInt(x + comboCount));
                                                 }
                                                 else if (x >= (comboCount - 0.5f))
                                                 {
                                                     CurrentItem = (EAavegotchiCombo)(Mathf.RoundToInt(x - comboCount));
                                                 }
                                                 else
                                                 {
                                                     CurrentItem = (EAavegotchiCombo)Mathf.RoundToInt(x);
                                                 }

                                                 CurrentPos = x;
                                                 UpdatePositions();
                                             },
                                             Mathf.Round(CurrentPos + shortestDistance), tweenDuration).SetEase(Ease.OutCubic));

            ActiveSequence.AppendCallback(() =>
            {
                var numItems = Enum.GetValues(typeof(EAavegotchiCombo)).Length;
                if (CurrentPos >= numItems)
                {
                    CurrentPos -= numItems;
                }
                else if (CurrentPos < 0)
                {
                    CurrentPos += numItems;
                }
                ActiveSequence.Kill();
                ActiveSequence = null;

                if (ItemBeforeTween != CurrentItem)
                {
                    AavegotchiPickerList[(int)ItemBeforeTween].AavegotchiRenderTextureHolder.AavegotchiAnimator.SetTrigger("Defeat");
                    AavegotchiPickerList[(int)CurrentItem].AavegotchiRenderTextureHolder.AavegotchiAnimator.SetTrigger("Victory_Fast");
                }
            });
        }

        //--------------------------------------------------------------------------------------------------
        private void UpdatePositions()
        {
            int itemCount = AavegotchiPickerList.Count;
            var halfCount = itemCount / 2;

            for (int i = 0; i < itemCount; i++)
            {
                var element = AavegotchiPickerList[i];

                // Calculate the offsetFromCenter based on CurrentPos and the index with wrapping
                float indexFromCenter = i - CurrentPos;

                // Adjust the offsetFromCenter to handle wrapping
                if (indexFromCenter > halfCount)
                {
                    indexFromCenter -= itemCount;
                }
                else if (indexFromCenter < -halfCount)
                {
                    indexFromCenter += itemCount;
                }

                SetPositionAndScale(element, indexFromCenter);
            }
        }

        //--------------------------------------------------------------------------------------------------
        private void CreateAllElements()
        {
            foreach (var comboId in Enum.GetValues(typeof(EAavegotchiCombo)))
            {
                var combo = (EAavegotchiCombo)comboId;

                var element = Instantiate(AavegotchiElementPrefab, ElementParent);
                element.transform.SetParent(ElementParent, false);
                element.SetupForCombo(combo);
                element.ParentPicker = this;

                AavegotchiPickerList.Add(element);
            }

            ElementWidth = 350;// (AavegotchiElementPrefab.transform as RectTransform).rect.width;

            UpdatePositions();
        }

        //--------------------------------------------------------------------------------------------------
        private void SetPositionAndScale(UIAavegotchiPicker_Element element, float indexFromCenter)
        {
            // Calculate the scale based on the indexFromCenter
            var maxVisibleElementsFromCenter = (NumVisibleElements - 1) / 2f;
            float cappedIndex = Mathf.Clamp(Mathf.Abs(indexFromCenter), 0f, maxVisibleElementsFromCenter) / maxVisibleElementsFromCenter;
            float scale = Mathf.Lerp(1.0f, EdgeScale, cappedIndex);

            // Set the element's scale
            element.transform.localScale = new Vector3(scale, scale, 1.0f);

            // Calculate the position based on the sign of indexFromCenter
            int floorVal = Math.Abs(Mathf.FloorToInt(indexFromCenter));
            float floorX = 0.0f;

            for (int i = 0; i < floorVal; ++i)
            {
                var indexScale = Mathf.Lerp(1.0f, EdgeScale, i / maxVisibleElementsFromCenter);
                var nextIndexScale = Mathf.Lerp(1.0f, EdgeScale, Mathf.Clamp(i + 1, 0.0f, maxVisibleElementsFromCenter) / maxVisibleElementsFromCenter);

                floorX += (ElementWidth * indexScale * 0.5f) + (ElementWidth * nextIndexScale * 0.5f) + Padding;
            }

            if (floorVal == indexFromCenter)
            {
                // Set the element's position
                element.transform.localPosition = new Vector3(floorX * MathF.Sign(indexFromCenter), 0.0f, 0.0f);
            }
            else
            {
                int ceilVal = Math.Abs(Mathf.CeilToInt(indexFromCenter));
                float ceilX = 0.0f;

                for (int i = 0; i < ceilVal; ++i)
                {
                    var indexScale = Mathf.Lerp(1.0f, EdgeScale, i / maxVisibleElementsFromCenter);
                    var nextIndexScale = Mathf.Lerp(1.0f, EdgeScale, Mathf.Clamp(i + 1, 0.0f, maxVisibleElementsFromCenter) / maxVisibleElementsFromCenter);

                    ceilX += (ElementWidth * indexScale * 0.5f) + (ElementWidth * nextIndexScale * 0.5f) + Padding;
                }

                int sign = MathF.Sign(indexFromCenter);
                float finalX = Mathf.Lerp(floorX, ceilX, indexFromCenter - floorVal * sign);
                element.transform.localPosition = new Vector3(finalX * sign, 0.0f, 0.0f);
            }
        }
    }
}