using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DressingRoomSectionItem : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] public Image Icon;
    [SerializeField] public Image SelectedHighlight;
    [SerializeField] public CanvasGroup CanvasGroup;
    [SerializeField] public Button Btn;
    [SerializeField] public string SelectedText;
}
