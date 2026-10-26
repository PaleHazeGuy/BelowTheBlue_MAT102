using UnityEngine;

public class ExtraCollectible : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Extra touched by: " + other.name);
        Debug.Log("Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            RunState.CollectExtra();
            Debug.Log("EXTRA COLLECTED!");

            Destroy(gameObject);
        }
    }
}