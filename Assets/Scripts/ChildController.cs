using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildController : MonoBehaviour
{
    private List<GameObject> childList = new List<GameObject>();
    private List<MeshRenderer> childMeshRendererList = new List<MeshRenderer>();

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            childList.Add(child.gameObject);
            childMeshRendererList.Add(child.GetComponent<MeshRenderer>());
        }
    }

    public void ChangeAllChildLayers(int layerIndex)
    {
        foreach (GameObject child in childList)
            child.layer = layerIndex;
    }

    public void ChangeAllChildMaterials(Material mat)
    {
        foreach (MeshRenderer mesh in childMeshRendererList)
            mesh.material = mat;
    }
}
