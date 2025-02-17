using UnityEngine;

public class ObjectInstantiation : MonoBehaviour
{
    public GameObject objectPrefab;

    void Start()
    {
        GameObject instantiatedObject = Instantiate(objectPrefab, transform.position, Quaternion.identity);
        instantiatedObject.transform.parent = transform;
        instantiatedObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
}
