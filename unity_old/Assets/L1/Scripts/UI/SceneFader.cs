
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// ระบบเฟดที่อยู่ข้าม Scene ได้
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning = false;

    private void Awake()
    {
        // ป้องกัน FadeManager ซ้ำ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            fadeGroup.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        // เผยหน้า Title เมื่อเริ่มเกม
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        if (isTransitioning) return;

        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isTransitioning = true;

        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;

            while (fadeGroup.alpha < 1f)
            {
                fadeGroup.alpha = Mathf.MoveTowards(
                    fadeGroup.alpha,
                    1f,
                    Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeDuration)
                );

                yield return null;
            }
        }

        SceneManager.LoadScene(sceneName);

        // รอให้ Scene ใหม่เริ่มทำงาน
        yield return null;

        if (fadeGroup != null)
        {
            while (fadeGroup.alpha > 0f)
            {
                fadeGroup.alpha = Mathf.MoveTowards(
                    fadeGroup.alpha,
                    0f,
                    Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeDuration)
                );

                yield return null;
            }

            fadeGroup.blocksRaycasts = false;
        }

        isTransitioning = false;
    }

    private IEnumerator FadeIn()
    {
        if (fadeGroup == null) yield break;

        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;

        while (fadeGroup.alpha > 0f)
        {
            fadeGroup.alpha = Mathf.MoveTowards(
                fadeGroup.alpha,
                0f,
                Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeDuration)
            );

            yield return null;
        }

        fadeGroup.blocksRaycasts = false;
    }
}
