
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// ควบคุมหน้าภาพและข้อความใน Lore
public class LoreController : MonoBehaviour
{
    [SerializeField] private Sprite[] illustrations;
    [SerializeField] private string[] storyTexts;
    [SerializeField] private Image illustrationImage;
    [SerializeField] private TMP_Text storyText;

    private int currentIndex = 0;

    private void Start()
    {
        ShowCurrentPage();
    }

    public void NextPage()
    {
        currentIndex++;

        // หากอ่านครบทุกหน้า ให้เริ่ม Gameplay
        if (currentIndex >= illustrations.Length)
        {
            SceneManager.LoadScene("Game");
            return;
        }

        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        if (illustrationImage != null &&
            currentIndex < illustrations.Length)
        {
            illustrationImage.sprite = illustrations[currentIndex];
        }

        if (storyText != null &&
            currentIndex < storyTexts.Length)
        {
            storyText.text = storyTexts[currentIndex];
        }
    }
}
