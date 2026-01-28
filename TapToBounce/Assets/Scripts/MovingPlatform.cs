using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 moveOffset = new Vector3(2, 0, 0);
    public float speed = 2f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + moveOffset * Mathf.Sin(Time.time * speed);
    }
}
