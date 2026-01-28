using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject platformPrefab;
    public GameObject obstaclePrefab;
    public GameObject coinPrefab;
    public GameObject shieldPrefab;

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        // Create Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, -4, 0);
        floor.transform.localScale = new Vector3(50, 1, 1);
        floor.GetComponent<Renderer>().material.color = Color.gray;
        
        // Make Floor Solid for 2D
        Destroy(floor.GetComponent<Collider>());
        floor.AddComponent<BoxCollider2D>();
        
        // Procedural Generation
        float currentX = -5f;
        float height = -2f;
        
        for (int i = 0; i < 20; i++)
        {
            currentX += Random.Range(2f, 4f);
            height += Random.Range(-1f, 1f);
            height = Mathf.Clamp(height, -2f, 2f);

            Vector3 pos = new Vector3(currentX, height, 0);
            
            // Randomly choose what to spawn
            float roll = Random.value;

            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.transform.position = pos;
            platform.transform.localScale = new Vector3(1.5f, 0.5f, 1f);
            platform.GetComponent<Renderer>().material.color = new Color(0.3f, 0.8f, 0.3f);

            // Make it solid for 2D Physics
            Destroy(platform.GetComponent<Collider>());
            platform.AddComponent<BoxCollider2D>();
            
            // Add Moving Platform script sometimes
            if (roll > 0.8f)
            {
                MovingPlatform mp = platform.AddComponent<MovingPlatform>();
                mp.moveOffset = new Vector3(0, 2f, 0);
                platform.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.8f);
            }

            // Spawn Obstacle on platform sometimes
            if (Random.value > 0.7f)
            {
                CreateObstacle(pos + Vector3.up * 0.5f);
            }
            // Spawn Coin sometimes
            else if (Random.value > 0.5f)
            {
                CreateCoin(pos + Vector3.up * 1.0f);
            }
             // Spawn Shield rarely
            else if (Random.value > 0.9f)
            {
                CreateShield(pos + Vector3.up * 1.0f);
            }
        }
    }

    void CreateObstacle(Vector3 pos)
    {
        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube); // Spike-ish
        obs.name = "Obstacle";
        obs.transform.position = pos;
        obs.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        obs.transform.rotation = Quaternion.Euler(0, 0, 45); // Rotate to look like spike
        obs.GetComponent<Renderer>().material.color = Color.red;
        
        // Primitives have MeshColliders (3D). We need BoxCollider2D for 2D physics interaction if the player is 2D.
        // Assuming Player is 2D Rigidbody.
        Destroy(obs.GetComponent<Collider>());
        BoxCollider2D bc = obs.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        
        obs.AddComponent<Obstacle>();
    }

    void CreateCoin(Vector3 pos)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Coin
        coin.name = "Coin";
        coin.transform.position = pos;
        coin.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        coin.GetComponent<Renderer>().material.color = Color.yellow;
        
        Destroy(coin.GetComponent<Collider>());
        CircleCollider2D cc = coin.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;

        PowerUp p = coin.AddComponent<PowerUp>();
        p.type = PowerUpType.Coin;
    }

    void CreateShield(Vector3 pos)
    {
        GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
        shield.name = "Shield";
        shield.transform.position = pos;
        shield.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        shield.GetComponent<Renderer>().material.color = Color.cyan;
        
        Destroy(shield.GetComponent<Collider>());
        CircleCollider2D cc = shield.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;

        PowerUp p = shield.AddComponent<PowerUp>();
        p.type = PowerUpType.Shield;
    }
}
