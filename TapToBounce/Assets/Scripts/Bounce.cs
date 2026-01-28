using UnityEngine;
using UnityEngine.SceneManagement;

public class Bounce : MonoBehaviour
{
    public float bounceForce = 5f;
    private Rigidbody2D rb;
    
    // State
    public int score = 0;
    public bool hasShield = false;
    public bool isDead = false;

    // Visuals
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        // Ensure SoundManager exists
        if (FindObjectOfType<SoundManager>() == null)
        {
            new GameObject("SoundManager").AddComponent<SoundManager>();
        }

        // Ensure LevelGenerator exists
        if (FindObjectOfType<LevelGenerator>() == null)
        {
            new GameObject("LevelGenerator").AddComponent<LevelGenerator>();
        }

        ImproveGraphics();

        // Squash & Stretch
        if (GetComponent<SquashStretch>() == null)
        {
            gameObject.AddComponent<SquashStretch>();
        }

        // Ensure rotation is allowed for visual effect
        rb.constraints = RigidbodyConstraints2D.None;
    }

    void Update()
    {
        if (isDead)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            rb.velocity = new Vector2(rb.velocity.x, bounceForce);
            SpawnJumpEffect();
            SoundManager.Instance?.PlayJump();

            // Add slight torque for fun rotation
            rb.AddTorque(-10f);
        }

        // Check if fell off
        if (transform.position.y < -10)
        {
            Die();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Trigger Squash
        SquashStretch ss = GetComponent<SquashStretch>();
        if (ss != null) ss.TriggerSquash();

        // Spawn Particles at contact point
        if (collision.contacts.Length > 0)
        {
            SpawnImpactEffect(collision.contacts[0].point, collision.contacts[0].normal);
        }
    }

    public void Die()
    {
        if (isDead) return;

        if (hasShield)
        {
            hasShield = false;
            SoundManager.Instance?.PlayShield();
            // Visual feedback for shield loss
            if (sr) sr.color = originalColor;
            return;
        }

        isDead = true;
        SoundManager.Instance?.PlayDie();
        Handheld.Vibrate();
        
        // Disable physics
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
    }

    public void CollectPowerUp(PowerUpType type)
    {
        if (type == PowerUpType.Coin)
        {
            score += 10;
            SoundManager.Instance?.PlayCoin();
        }
        else if (type == PowerUpType.Shield)
        {
            hasShield = true;
            SoundManager.Instance?.PlayShield();
            if (sr) sr.color = Color.cyan; // Visual indicator
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(20, 20, 300, 50), "Score: " + score, style);

        if (isDead)
        {
            style.fontSize = 60;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(Screen.width/2 - 150, Screen.height/2 - 50, 300, 100), "GAME OVER\nTap to Restart", style);
        }
    }

    void ImproveGraphics()
    {
        // 1. Add Realistic Background
        CreateRealisticBackground();

        // 2. Enhance Player Visuals
        if (sr != null)
        {
            sr.color = new Color(1f, 0.4f, 0.5f); // Coral/Pinkish
            originalColor = sr.color;
        }

        // 3. Add Trail
        if (GetComponent<TrailRenderer>() == null)
        {
            TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.4f;
            trail.startWidth = 0.6f;
            trail.endWidth = 0.0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(1f, 0.4f, 0.5f, 0.6f);
            trail.endColor = new Color(1f, 0.9f, 0.6f, 0f);
            trail.sortingOrder = -1; 
        }

        AddFace();
    }

    void AddFace()
    {
        if (transform.Find("Face") != null) return;

        GameObject face = new GameObject("Face");
        face.transform.SetParent(transform);
        face.transform.localPosition = Vector3.zero;
        face.transform.localScale = Vector3.one;

        // Generate Textures
        Texture2D eyeTex = CreateCircleTexture(64, Color.white);
        Texture2D pupilTex = CreateCircleTexture(32, Color.black);
        
        Sprite eyeSprite = Sprite.Create(eyeTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        Sprite pupilSprite = Sprite.Create(pupilTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        // Left Eye
        GameObject leftEye = new GameObject("LeftEye");
        leftEye.transform.SetParent(face.transform);
        leftEye.transform.localPosition = new Vector3(-0.15f, 0.15f, 0);
        leftEye.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        SpriteRenderer leSr = leftEye.AddComponent<SpriteRenderer>();
        leSr.sprite = eyeSprite;
        leSr.sortingOrder = 5;

        GameObject leftPupil = new GameObject("Pupil");
        leftPupil.transform.SetParent(leftEye.transform);
        leftPupil.transform.localPosition = new Vector3(0.1f, -0.1f, 0); 
        SpriteRenderer lpSr = leftPupil.AddComponent<SpriteRenderer>();
        lpSr.sprite = pupilSprite;
        lpSr.sortingOrder = 6;

        // Right Eye
        GameObject rightEye = new GameObject("RightEye");
        rightEye.transform.SetParent(face.transform);
        rightEye.transform.localPosition = new Vector3(0.15f, 0.15f, 0);
        rightEye.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        SpriteRenderer reSr = rightEye.AddComponent<SpriteRenderer>();
        reSr.sprite = eyeSprite;
        reSr.sortingOrder = 5;
        
        GameObject rightPupil = new GameObject("Pupil");
        rightPupil.transform.SetParent(rightEye.transform);
        rightPupil.transform.localPosition = new Vector3(0.1f, -0.1f, 0);
        SpriteRenderer rpSr = rightPupil.AddComponent<SpriteRenderer>();
        rpSr.sprite = pupilSprite;
        rpSr.sortingOrder = 6;
        
        // Mouth
        GameObject mouth = new GameObject("Mouth");
        mouth.transform.SetParent(face.transform);
        mouth.transform.localPosition = new Vector3(0, -0.1f, 0);
        mouth.transform.localScale = new Vector3(0.25f, 0.1f, 1f);
        SpriteRenderer mSr = mouth.AddComponent<SpriteRenderer>();
        mSr.sprite = pupilSprite; // Black circle
        mSr.sortingOrder = 5;
    }

    Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        // Transparent background
        for (int i = 0; i < size * size; i++) tex.SetPixel(i % size, i / size, Color.clear);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = (size / 2f) - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }
        tex.Apply();
        return tex;
    }

    void CreateRealisticBackground()
    {
        if (GameObject.Find("RealisticBackground") != null) return;

        Texture2D bgTex = Resources.Load<Texture2D>("Background");
        
        if (bgTex != null)
        {
            GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bgObj.name = "RealisticBackground";
            Destroy(bgObj.GetComponent<MeshCollider>()); 

            Renderer rend = bgObj.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Unlit/Texture"));
            rend.material.mainTexture = bgTex;

            if (Camera.main != null)
            {
                if (Camera.main.orthographic)
                {
                    float height = 2f * Camera.main.orthographicSize;
                    float width = height * Camera.main.aspect;
                    bgObj.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 10f);
                    bgObj.transform.localScale = new Vector3(width, height, 1f);
                }
                else
                {
                    float dist = 50f;
                    bgObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * dist;
                    float height = 2.0f * Mathf.Tan(0.5f * Camera.main.fieldOfView * Mathf.Deg2Rad) * dist;
                    float width = height * Camera.main.aspect;
                    bgObj.transform.rotation = Camera.main.transform.rotation;
                    bgObj.transform.localScale = new Vector3(width, height, 1f);
                }
            }
        }
        else
        {
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.1f, 0.1f, 0.25f);
            }
        }
    }

    void SpawnJumpEffect()
    {
        GameObject pObj = new GameObject("JumpBurst");
        pObj.transform.position = transform.position;
        
        ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.9f, 0.8f); 
        
        var emission = ps.emission;
        emission.enabled = false; 
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;

        ParticleSystemRenderer psr = pObj.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Sprites/Default"));

        ps.Emit(15);
        Destroy(pObj, 1f);
    }

    void SpawnImpactEffect(Vector2 position, Vector2 normal)
    {
        GameObject pObj = new GameObject("ImpactBurst");
        pObj.transform.position = position;
        pObj.transform.up = normal;
        
        ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.startLifetime = 0.3f;
        main.startSpeed = 5f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 1f, 0.5f); 
        
        var emission = ps.emission;
        emission.enabled = false; 
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 60f;

        ParticleSystemRenderer psr = pObj.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Sprites/Default"));

        ps.Emit(25);
        Destroy(pObj, 1f);
    }
}
