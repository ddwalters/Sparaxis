using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineBrain))]
public class PixelSnapCamera : MonoBehaviour
{
    [SerializeField] private float pixelsPerUnit = 16f;

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x * pixelsPerUnit) / pixelsPerUnit;
        pos.y = Mathf.Round(pos.y * pixelsPerUnit) / pixelsPerUnit;
        transform.position = pos;
    }
}
