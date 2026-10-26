using UnityEngine;

public class WinGoal : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("WinGoal touched by: " + other.name);

        if (triggered || !other.CompareTag("Player"))
            return;

        triggered = true;

        Debug.Log("WIN GOAL REACHED!");

        RunState.Win();
    }
}