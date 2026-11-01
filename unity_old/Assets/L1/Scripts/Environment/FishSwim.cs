
using UnityEngine;

// ให้ปลาว่ายไปกลับในระยะที่กำหนด
public class FishSwim : MonoBehaviour
{
    [SerializeField] private float swimDistance = 2f;
    [SerializeField] private float swimSpeed = 1f;

    private Vector3 startPosition;
    private int direction = 1;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position += Vector3.right
            * direction * swimSpeed * Time.deltaTime;

        float distance = transform.position.x - startPosition.x;

        if (distance >= swimDistance)
        {
            direction = -1;
            FlipFish(false);
        }
        else if (distance <= -swimDistance)
        {
            direction = 1;
            FlipFish(true);
        }
    }

    private void FlipFish(bool faceRight)
    {
        Vector3 scale = transform.localScale;

        // ใช้ขนาดเดิมและเปลี่ยนเฉพาะทิศที่หัน
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1 : -1);

        transform.localScale = scale;
    }
}
