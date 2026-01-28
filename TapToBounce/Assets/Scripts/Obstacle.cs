using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Bounce player = other.GetComponent<Bounce>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}
