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

    private int spinDirection = 1;
    private Vector3 originalScale;
    private Transform robotVisuals;
    private float targetRotation = 0f;

    void Start()
    {
        gameObject.tag = "Player";
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

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
        SetupPhysics();
        CreateScreenBoundaries();

        // Squash & Stretch
        if (GetComponent<SquashStretch>() == null)
        {
            gameObject.AddComponent<SquashStretch>();
        }

        // Ensure rotation is allowed for visual effect
        rb.constraints = RigidbodyConstraints2D.None;
    }

    void SetupPhysics()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        
        // Add bounciness
        PhysicsMaterial2D mat = new PhysicsMaterial2D("BouncyPlayer");
        mat.bounciness = 0.8f;
        mat.friction = 0.1f;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.sharedMaterial = mat;
        }

        // Add some initial horizontal push
        rb.velocity = new Vector2(2f, 0);
    }

    void CreateScreenBoundaries()
    {
        if (Camera.main == null) return;

        float screenHeight = 2f * Camera.main.orthographicSize;
        float screenWidth = screenHeight * Camera.main.aspect;

        PhysicsMaterial2D wallMat = new PhysicsMaterial2D("BouncyWall");
        wallMat.bounciness = 1.0f;
        wallMat.friction = 0f;

        // Left Wall
        CreateWall("LeftWall", new Vector3(-screenWidth / 2f - 0.5f, 0, 0), new Vector2(1, 100), wallMat);
        // Right Wall
        CreateWall("RightWall", new Vector3(screenWidth / 2f + 0.5f, 0, 0), new Vector2(1, 100), wallMat);
        // Top Wall (Ceiling)
        CreateWall("TopWall", new Vector3(0, screenHeight / 2f + 0.5f, 0), new Vector2(100, 1), wallMat);
        // Bottom Wall (Floor)
        CreateWall("BottomWall", new Vector3(0, -screenHeight / 2f - 0.5f, 0), new Vector2(100, 1), wallMat);
    }

    void CreateWall(string name, Vector3 localPos, Vector2 size, PhysicsMaterial2D mat)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(Camera.main.transform);
        wall.transform.localPosition = localPos;
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = size;
        col.sharedMaterial = mat;
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

        HandleCameraFollow();
        HandleFloorConstraint();

        if (Input.GetMouseButtonDown(0))
        {
            Jump();
        }

        // Procedural Jump Animation (Smoothing back to original scale)
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 5f);
        
        // Proper Spin Animation: Smoothly rotate the visual container
        if (robotVisuals != null)
        {
            float curRot = robotVisuals.localEulerAngles.z;
            float nextRot = Mathf.LerpAngle(curRot, targetRotation, Time.deltaTime * 10f);
            robotVisuals.localEulerAngles = new Vector3(0, 0, nextRot);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, bounceForce);
        SpawnJumpEffect();
        SoundManager.Instance?.PlayJump();

        // Procedural Jump Animation: Stretch on jump
        transform.localScale = new Vector3(originalScale.x * 0.7f, originalScale.y * 1.4f, originalScale.z);

        // Control Spin Direction: Alternate 360 degree flip
        spinDirection *= -1;
        targetRotation += spinDirection * 360f;
    }

    void HandleFloorConstraint()
    {
        // "Cannot go lower than the main stage"
        // Stage floor is at y = -4 (from LevelGenerator). 
        // We'll set a hard bound at y = -3.5 (assuming character size ~1).
        if (transform.position.y < -3.5f)
        {
            Vector3 pos = transform.position;
            pos.y = -3.5f;
            transform.position = pos;
            
            // Bounce up if hitting floor logic-wise
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Abs(rb.velocity.y) * 0.5f);
            }
        }
    }

    void HandleCameraFollow()
    {
        if (Camera.main != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            // Only follow on X axis to show progression
            camPos.x = Mathf.Lerp(camPos.x, transform.position.x, Time.deltaTime * 5f);
            Camera.main.transform.position = camPos;
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
        if (transform.Find("RobotVisuals") != null) return;

        GameObject visObj = new GameObject("RobotVisuals");
        visObj.transform.SetParent(transform);
        visObj.transform.localPosition = Vector3.zero;
        visObj.transform.localScale = Vector3.one;
        robotVisuals = visObj.transform;

        // Robotic Body (Lower)
        GameObject body = CreatePart("Torso", new Vector3(0, -0.2f, 0), new Vector3(0.8f, 0.6f, 1f), new Color(0.5f, 0.5f, 0.6f), robotVisuals);
        
        // Robotic Head (Upper)
        GameObject head = CreatePart("Head", new Vector3(0, 0.25f, 0), new Vector3(0.7f, 0.5f, 1f), new Color(0.7f, 0.7f, 0.8f), robotVisuals);

        // Antennas
        CreatePart("AntennaL", new Vector3(-0.2f, 0.6f, 0), new Vector3(0.05f, 0.3f, 1f), Color.gray, robotVisuals);
        CreatePart("AntennaR", new Vector3(0.2f, 0.6f, 0), new Vector3(0.05f, 0.3f, 1f), Color.gray, robotVisuals);
        CreatePart("TipL", new Vector3(-0.2f, 0.75f, 0), new Vector3(0.12f, 0.12f, 1f), Color.red, robotVisuals);
        CreatePart("TipR", new Vector3(0.2f, 0.75f, 0), new Vector3(0.12f, 0.12f, 1f), Color.red, robotVisuals);

        // Robotic Arms (Tucked in)
        CreatePart("ArmL", new Vector3(-0.45f, -0.1f, 0), new Vector3(0.2f, 0.4f, 1f), new Color(0.4f, 0.4f, 0.5f), robotVisuals);
        CreatePart("ArmR", new Vector3(0.45f, -0.1f, 0), new Vector3(0.2f, 0.4f, 1f), new Color(0.4f, 0.4f, 0.5f), robotVisuals);

        // Visor
        GameObject visor = CreatePart("Visor", new Vector3(0, 0.3f, 0), new Vector3(0.55f, 0.2f, 1f), new Color(0.1f, 0.1f, 0.1f), robotVisuals);
        
        // Glow Eyes
        Texture2D glowTex = CreateCircleTexture(32, Color.cyan);
        Sprite glowSprite = Sprite.Create(glowTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        GameObject eyeL = new GameObject("EyeL");
        eyeL.transform.SetParent(visor.transform);
        eyeL.transform.localPosition = new Vector3(-0.25f, 0, 0);
        eyeL.transform.localScale = new Vector3(0.3f, 0.6f, 1f);
        eyeL.AddComponent<SpriteRenderer>().sprite = glowSprite;
        eyeL.GetComponent<SpriteRenderer>().sortingOrder = 10;

        GameObject eyeR = new GameObject("EyeR");
        eyeR.transform.SetParent(visor.transform);
        eyeR.transform.localPosition = new Vector3(0.25f, 0, 0);
        eyeR.transform.localScale = new Vector3(0.3f, 0.6f, 1f);
        eyeR.AddComponent<SpriteRenderer>().sprite = glowSprite;
        eyeR.GetComponent<SpriteRenderer>().sortingOrder = 10;

        // Hide original sprite renderer if it exists to avoid overlap
        if (sr != null) sr.enabled = false;
    }

    GameObject CreatePart(string name, Vector3 pos, Vector3 scale, Color color, Transform parent)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        Destroy(part.GetComponent<Collider>());
        part.transform.SetParent(parent);
        part.transform.localPosition = pos;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().material = new Material(Shader.Find("Sprites/Default"));
        part.GetComponent<Renderer>().material.color = color;
        return part;
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
                    bgObj.transform.SetParent(Camera.main.transform);
                    bgObj.transform.localPosition = new Vector3(0, 0, 10f);
                    bgObj.transform.localScale = new Vector3(width, height, 1f);
                }
                else
                {
                    float dist = 50f;
                    bgObj.transform.SetParent(Camera.main.transform);
                    bgObj.transform.localPosition = Vector3.forward * dist;
                    float height = 2.0f * Mathf.Tan(0.5f * Camera.main.fieldOfView * Mathf.Deg2Rad) * dist;
                    float width = height * Camera.main.aspect;
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
