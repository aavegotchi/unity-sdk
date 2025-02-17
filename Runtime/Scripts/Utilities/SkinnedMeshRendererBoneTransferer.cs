using UnityEngine;

public class SkinnedMeshRendererBoneTransferer : MonoBehaviour
{
    [SerializeField]
    SkinnedMeshRenderer RootRenderer;

    [SerializeField]
    SkinnedMeshRenderer TargetRenderer;

    //--------------------------------------------------------------------------------------------------
    public void TransferBones()
    {
        TargetRenderer.bones = RootRenderer.bones;
        TargetRenderer.rootBone = RootRenderer.rootBone;
    }
}
