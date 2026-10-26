using UnityEngine;                     // ใช้คลาส MonoBehaviour และ Application
using UnityEngine.SceneManagement;     // ใช้โหลด Scene

public class SceneButtons : MonoBehaviour // สคริปต์สำหรับเชื่อมกับปุ่ม UI
{
    public void StartGame()             // ฟังก์ชันสำหรับปุ่ม START
    {
        RunState.ResetRun();            // ล้างข้อมูลจากรอบก่อน
        SceneManager.LoadScene("Lore"); // ไปหน้าเนื้อเรื่อง
    }

    public void GoToGameplay()          // ฟังก์ชันสำหรับเริ่ม Gameplay โดยตรง
    {
        SceneManager.LoadScene("Game"); // เปิดฉากเกมหลัก
    }

    public void RestartToTitle()        // ฟังก์ชันสำหรับปุ่ม RESTART
    {
        RunState.ResetRun();            // ล้างข้อมูลรอบเดิม
        SceneManager.LoadScene("Title"); // กลับหน้าเปิดเกม
    }

    public void QuitGame()              // ฟังก์ชันสำหรับปุ่ม QUIT
    {
        Application.Quit();             // ปิดเกมเมื่อเล่นจากตัวเกมที่ Build แล้ว
    }
}