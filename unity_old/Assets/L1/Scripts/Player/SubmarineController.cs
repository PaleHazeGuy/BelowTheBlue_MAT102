
using UnityEngine;

// ควบคุมการเคลื่อนที่ของเรือ
[RequireComponent(typeof(Rigidbody2D))]
public class SubmarineController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // เริ่มรอบใหม่ทุกครั้งที่เข้าฉากเกม เพื่อไม่ให้สถานะจบเกมค้างจากรอบก่อน
        RunState.ResetRun();
    }

    private void Update()
    {
        // อ่านปุ่ม WASD
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // จำกัดความเร็วแนวทแยงไม่ให้เร็วกว่าทิศตรง
        moveInput = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }

    private void FixedUpdate()
    {
        // หยุดการควบคุมเมื่อเกมจบ
        if (RunState.HasEnded) return;

        // เคลื่อนเรือด้วยระบบฟิสิกส์ 2D
        rb.MovePosition(
            rb.position + moveInput * moveSpeed * Time.fixedDeltaTime
        );
    }
}
