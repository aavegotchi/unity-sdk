using DG.Tweening;
using UnityEngine;

namespace GotchiSDK.UI
{
    public class UIElement_Visibility : MonoBehaviour
    {
        public enum VisibilityState
        {
            Visible,
            Showing,
            Hidden,
            Hiding,
        }

        [Header("Visibility")]
        [SerializeField]
        private VisibilityState DefaultState = VisibilityState.Hidden;
        private VisibilityState _currentState = VisibilityState.Hidden;
        public VisibilityState CurrentState
        {
            get
            {
                return _currentState;
            }
            private set
            {
                _currentState = value;
                DEBUG_TargetState = value;
            }
        }

        [SerializeField]
        private VisibilityState DEBUG_TargetState = VisibilityState.Hidden;

        [SerializeField]
        protected float ShowDuration = 0.7f;

        // This value lets us have the tween durations set to the time its taken so far to show/hide when switching modes mid-animation
        private float CurrentVisibilityProgress = 0f;

        protected Sequence VisibilityTweeners = null;

        //--------------------------------------------------------------------------------------------------
        protected virtual void Awake()
        {
            if (DefaultState == VisibilityState.Visible || DefaultState == VisibilityState.Hiding)
            {
                CurrentState = VisibilityState.Visible;
                CurrentVisibilityProgress = ShowDuration;
                OnShown();
            }
            else if (DefaultState == VisibilityState.Hidden || DefaultState == VisibilityState.Showing)
            {
                CurrentState = VisibilityState.Hidden;
                CurrentVisibilityProgress = 0f;
                OnHidden();
            }

            if (DefaultState == VisibilityState.Showing)
            {
                Show();
            }
            else if (DefaultState == VisibilityState.Hiding)
            {
                Hide();
            }
        }

        //--------------------------------------------------------------------------------------------------
        protected virtual void OnDestroy()
        {
            if (VisibilityTweeners != null)
            {
                VisibilityTweeners.Kill();
                VisibilityTweeners = null;
            }
        }

        //--------------------------------------------------------------------------------------------------
        public virtual void Update()
        {
            if (DEBUG_TargetState != CurrentState)
            {
                if (DEBUG_TargetState == VisibilityState.Visible || DEBUG_TargetState == VisibilityState.Showing)
                {
                    Show();
                }
                else if (DEBUG_TargetState == VisibilityState.Hidden || DEBUG_TargetState == VisibilityState.Hiding)
                {
                    Hide();
                }
            }
        }

        //--------------------------------------------------------------------------------------------------
        public bool IsHiddenOrHiding() { return CurrentState == VisibilityState.Hidden || CurrentState == VisibilityState.Hiding; }

        //--------------------------------------------------------------------------------------------------
        public void Show()
        {
            if (CurrentState == VisibilityState.Visible || CurrentState == VisibilityState.Showing)
            {
                return;
            }

            StartShowInternal();
        }

        //--------------------------------------------------------------------------------------------------
        public void Hide()
        {
            if (CurrentState == VisibilityState.Hidden || CurrentState == VisibilityState.Hiding)
            {
                return;
            }

            StartHideInternal();
        }

        //--------------------------------------------------------------------------------------------------
        // Override to make sure the objects are properly shown
        // Both at end of animations but also startup
        //--------------------------------------------------------------------------------------------------
        protected virtual void OnShown()
        {

        }

        //--------------------------------------------------------------------------------------------------
        // Override to make sure the objects are properly Hidden
        // Both at end of animations but also startup
        //--------------------------------------------------------------------------------------------------
        protected virtual void OnHidden()
        {

        }

        //--------------------------------------------------------------------------------------------------
        // Override this function to add custom animations
        // Modify ShowDuration if the animation time is dynamic for best results
        //--------------------------------------------------------------------------------------------------
        protected virtual void SpawnShowAnimations(float currentValue, float duration, Sequence masterSequence)
        {
        }

        //--------------------------------------------------------------------------------------------------
        // Override this function to add custom animations
        // Modify ShowDuration if the animation time is dynamic for best results
        //--------------------------------------------------------------------------------------------------
        protected virtual void SpawnHideAnimations(float currentValue, float duration, Sequence masterSequence)
        {
        }

        //--------------------------------------------------------------------------------------------------
        private void StartShowInternal()
        {
            if (VisibilityTweeners != null)
            {
                VisibilityTweeners.Pause();
                VisibilityTweeners.Kill();
                VisibilityTweeners = null;
            }

            CurrentState = VisibilityState.Showing;

            VisibilityTweeners = DOTween.Sequence();

            float duration = ShowDuration - CurrentVisibilityProgress;

            SpawnShowAnimations(CurrentVisibilityProgress, duration, VisibilityTweeners);

            VisibilityTweeners.Insert(0.0f, DOTween.To(() => CurrentVisibilityProgress, x => CurrentVisibilityProgress = x, ShowDuration, duration));
            VisibilityTweeners.AppendCallback(() =>
            {
                CurrentState = VisibilityState.Visible;
                VisibilityTweeners.Kill();
                VisibilityTweeners = null;
                OnShown();
            });
        }

        //--------------------------------------------------------------------------------------------------
        private void StartHideInternal()
        {
            if (VisibilityTweeners != null)
            {
                VisibilityTweeners.Pause();
                VisibilityTweeners.Kill();
                VisibilityTweeners = null;
            }

            CurrentState = VisibilityState.Hiding;

            VisibilityTweeners = DOTween.Sequence();

            SpawnHideAnimations(CurrentVisibilityProgress, CurrentVisibilityProgress, VisibilityTweeners);

            VisibilityTweeners.Insert(0.0f, DOTween.To(() => CurrentVisibilityProgress, x => CurrentVisibilityProgress = x, 0f, CurrentVisibilityProgress));
            VisibilityTweeners.AppendCallback(() =>
            {
                CurrentState = VisibilityState.Hidden;
                VisibilityTweeners.Kill();
                VisibilityTweeners = null;
                OnHidden();
            });
        }
    }
}