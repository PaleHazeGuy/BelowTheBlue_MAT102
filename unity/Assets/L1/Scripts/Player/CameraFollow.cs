
using UnityEngine;

// ทำให้กล้องติดตามเรือ
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float verticalOffset = 1.5f;

    private void LateUpdate()
    {
        if (target == null) return;

        // กล้องตามแกน X และ Y แต่คง Z ไว้ที่ -10
        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y + verticalOffset,
            -10f
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }
}
