using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GG.Gameplay.UI
{
    public class AdvancedButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private Image TargetHoldOverlay;
        [SerializeField] private float HoldDuration = 1.0f;
        [SerializeField] private float MinHoldAnimDuration = 0.2f;
        [SerializeField] private float DragAndDropThreshhold = 5.0f;
        [SerializeField] private bool DragAndDropEnabled = false;
        [SerializeField] private bool PressAndHoldEnabled = false;

        [SerializeField] public Image BtnBG = null;
        [SerializeField] public Sprite NormalBG;
        [SerializeField] public Sprite HoveredBG;
        [SerializeField] public Sprite PressedBG;
        [SerializeField] public Sprite DisabledBG;
        [SerializeField] public GameObject SelectedTabHighlightRoot;

        public UnityEvent OnPressAndHoldFinished;
        public UnityEvent OnDragStarted;
        public UnityEvent OnDragUpdated;
        public UnityEvent OnDragEnd;
        public UnityEvent OnDown;
        public UnityEvent OnUp;
        public UnityEvent OnEnter = new UnityEvent();
        public UnityEvent OnExit = new UnityEvent();

        public bool Hovering => _IsCursorOver;

        public Vector2 CurrentDragLocation;

        private Button MatchingButton;
        private Sequence PressSequence;
        private Vector2 _startPosition;
        private bool IsPressAndHoldActive = false;
        private bool _IsDragging = false;
        private bool _Pressed = false;
        private bool _IsCursorOver = false;

        //--------------------------------------------------------------------------------------------------
        public void OnEnable()
        {
            if (BtnBG != null)
            {
                if (_Pressed)
                {
                    BtnBG.sprite = PressedBG;
                }
                else if (_IsCursorOver)
                {
                    BtnBG.sprite = HoveredBG;
                }
                else
                {
                    BtnBG.sprite = NormalBG;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void OnDisable()
        {
            if (BtnBG != null)
            {
                BtnBG.sprite = DisabledBG != null ? DisabledBG : NormalBG;
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void UpdateNormalBG(Sprite normalBG)
        {
            if (NormalBG != normalBG)
            {
                NormalBG = normalBG;

                if (!_IsCursorOver && !_Pressed)
                {
                    BtnBG.sprite = NormalBG;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void SetPressAndHolderEnabled(bool val)
        {
            if (PressAndHoldEnabled != val)
            {
                PressAndHoldEnabled = val;

                if (!PressAndHoldEnabled && PressSequence != null)
                {
                    if (MatchingButton != null)
                    {
                        MatchingButton.enabled = true;
                    }
                    PressSequence.Kill();
                    PressSequence = null;

                    TargetHoldOverlay.fillAmount = 0.0f;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void SetDragEnabled(bool val)
        {
            if (DragAndDropEnabled != val)
            {
                DragAndDropEnabled = val;

                if (_IsDragging && !DragAndDropEnabled)
                {
                    if (MatchingButton != null)
                    {
                        MatchingButton.enabled = true;
                    }

                    _IsDragging = false;
                    OnDragEnd?.Invoke();
                }
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void OnPointerDown(PointerEventData eventData)
        {
            if (MatchingButton == null)
            {
                MatchingButton = GetComponent<Button>();
            }

            _Pressed = true;

            if (BtnBG != null)
            {
                BtnBG.sprite = PressedBG;
            }

            _startPosition = eventData.position;

            if (MatchingButton != null)
            {
                MatchingButton.enabled = true;
            }

            IsPressAndHoldActive = true;

            if (PressAndHoldEnabled)
            {
                if (PressSequence != null)
                {
                    PressSequence.Kill();
                    PressSequence = null;
                }

                TargetHoldOverlay.fillAmount = 0.0f;

                PressSequence = DOTween.Sequence();

                PressSequence.Insert(0.0f, TargetHoldOverlay.DOFillAmount(1.0f, HoldDuration));

                PressSequence.InsertCallback(MinHoldAnimDuration, () =>
                {
                    if (!IsPressAndHoldActive && PressSequence != null)
                    {
                        TargetHoldOverlay.fillAmount = 0.0f;
                        PressSequence.Kill();
                        PressSequence = null;
                    }
                    else
                    {
                        if (MatchingButton != null)
                        {
                            MatchingButton.enabled = false;
                        }
                    }
                });

                PressSequence.AppendCallback(() =>
                {
                    PressSequence = null;
                    TargetHoldOverlay.fillAmount = 0.0f;
                    OnPressAndHoldFinished?.Invoke();
                });
            }

            OnDown?.Invoke();
        }

        //--------------------------------------------------------------------------------------------------
        public void OnDrag(PointerEventData eventData)
        {
            if (!DragAndDropEnabled)
            {
                return;
            }

            if (_IsDragging)
            {
                if (CurrentDragLocation != eventData.position)
                {
                    CurrentDragLocation = eventData.position;
                    OnDragUpdated?.Invoke();
                }
            }
            else if (Vector2.Distance(_startPosition, eventData.position) > DragAndDropThreshhold)
            {
                if (PressSequence != null)
                {
                    PressSequence.Kill();
                    PressSequence = null;
                    TargetHoldOverlay.fillAmount = 0.0f;
                }

                if (MatchingButton != null)
                {
                    MatchingButton.enabled = false;
                }

                CurrentDragLocation = eventData.position;
                _IsDragging = true;
                OnDragStarted?.Invoke();
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressAndHoldActive = false;

            _Pressed = false;

            if (BtnBG != null)
            {
                BtnBG.sprite = _IsCursorOver ? HoveredBG : NormalBG;
            }

            if (_IsDragging && DragAndDropEnabled)
            {
                CurrentDragLocation = eventData.position;
                _IsDragging = false;
                OnDragEnd?.Invoke();
            }

            if (PressSequence != null && PressSequence.Elapsed() >= MinHoldAnimDuration)
            {
                PressSequence.Kill();
                PressSequence = null;

                TargetHoldOverlay.fillAmount = 0.0f;
            }

            OnUp?.Invoke();
        }

        //--------------------------------------------------------------------------------------------------
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (BtnBG != null && !_Pressed)
            {
                BtnBG.sprite = HoveredBG;
            }

            _IsCursorOver = true;

            OnEnter?.Invoke();
        }

        //--------------------------------------------------------------------------------------------------
        public void OnPointerExit(PointerEventData eventData)
        {
            _IsCursorOver = false;

            if (BtnBG != null && !_Pressed)
            {
                BtnBG.sprite = NormalBG;
            }

            OnExit?.Invoke();
        }
    }
}