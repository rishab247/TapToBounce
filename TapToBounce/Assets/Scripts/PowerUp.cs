using UnityEngine;

public enum PowerUpType { Shield, Coin }

public class PowerUp : MonoBehaviour
{
    public PowerUpType type;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Bounce player = other.GetComponent<Bounce>();
            if (player != null)
            {
                player.CollectPowerUp(type);
                Destroy(gameObject);
            }
        }
    }
}
