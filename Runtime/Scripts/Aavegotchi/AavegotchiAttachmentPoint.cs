using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AavegotchiAttachmentPoint
{
    public string Name;
    public Transform Target;
}

[Serializable]
public class WearableAttachment
{
    public string Name;
    public GameObject ToAttach;
}