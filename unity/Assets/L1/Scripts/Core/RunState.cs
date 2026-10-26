
using UnityEngine;
using UnityEngine.SceneManagement;

// เก็บข้อมูลส่วนกลางของเกมในแต่ละรอบ
public static class RunState
{
    // จำนวน Extra ที่ผู้เล่นเก็บได้
    public static int ExtraCount { get; private set; } = 0;

    // จำนวนครั้งที่เรือชนหิน
    public static int HitCount { get; private set; } = 0;

    // ตรวจสอบว่าเกมจบแล้วหรือยัง
    public static bool HasEnded { get; private set; } = false;

    // รีเซ็ตข้อมูลเมื่อเริ่มรอบใหม่
    public static void ResetRun()
    {
        ExtraCount = 0;
        HitCount = 0;
        HasEnded = false;
    }

    // เพิ่มจำนวน Extra เมื่อผู้เล่นเก็บของ
    public static void CollectExtra()
    {
        if (HasEnded) return;

        ExtraCount++;

        // จำกัดจำนวน Extra สูงสุดไว้ที่ 3
        ExtraCount = Mathf.Min(ExtraCount, 3);
    }

    // เพิ่มจำนวนครั้งที่ชนหิน
    public static int RegisterRockHit()
    {
        if (HasEnded) return HitCount;

        HitCount++;

        // ชนหินครบ 3 ครั้งให้แพ้
        if (HitCount >= 3)
        {
            Lose();
        }

        // ส่งจำนวนครั้งที่ชนกลับไปให้ระบบเรือ
        return HitCount;
    }

    // จบเกมเมื่อผู้เล่นแพ้
    public static void Lose()
    {
        if (HasEnded) return;

        HasEnded = true;

        SceneManager.LoadScene("GameOver");
    }

    // จบเกมเมื่อผู้เล่นชนะ
    public static void Win()
    {
        if (HasEnded) return;

        HasEnded = true;

        SceneManager.LoadScene("Ending");
    }
}
