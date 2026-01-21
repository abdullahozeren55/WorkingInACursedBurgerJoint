using UnityEngine;
using static UnityEditor.Progress;

public class GrillTriggerRelay : MonoBehaviour
{
    [SerializeField] private Grill parentGrill;

    private void OnTriggerEnter(Collider other)
    {
        // Tetiklenince ana sepetteki fonksiyonu çaðýr
        if (parentGrill != null)
        {
            parentGrill.AttemptAddItem(other);
        }
    }
}
