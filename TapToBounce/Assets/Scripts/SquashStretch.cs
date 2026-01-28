using UnityEngine;

public class SquashStretch : MonoBehaviour
{
    public float maxStretch = 1.5f;
    public float maxSquash = 0.5f;
    public float stretchSpeed = 5f;
    public float returnSpeed = 10f;

    private Rigidbody2D rb;
    private Vector3 originalScale;
    private bool isSquashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isSquashing) return;

        // Stretch based on Y velocity
        float velocityY = rb.velocity.y;
        float stretchAmount = Mathf.Abs(velocityY) * 0.05f;
        float scaleY = originalScale.y + stretchAmount;
        float scaleX = originalScale.x - (stretchAmount * 0.5f); // Maintain volume roughly

        // Clamp
        scaleY = Mathf.Clamp(scaleY, originalScale.y, originalScale.y * maxStretch);
        scaleX = Mathf.Clamp(scaleX, originalScale.x * maxSquash, originalScale.x);

        // Smoothly interpolate
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(scaleX, scaleY, originalScale.z), Time.deltaTime * stretchSpeed);
        
        // Face direction logic (if we have a Rigidbody)
        // If moving right, rotate/look right? The requirement is "Rotation: Ensure the ball rotates visually". 
        // If physics rotation is used, we don't need to manually rotate, but we should ensure angular drag isn't too high.
    }

    public void TriggerSquash(float intensity = 1f)
    {
        if (isSquashing) return;
        StartCoroutine(SquashRoutine(intensity));
    }

    private System.Collections.IEnumerator SquashRoutine(float intensity)
    {
        isSquashing = true;
        
        float squashY = originalScale.y * (maxSquash / intensity); // More intensity = more squash
        float squashX = originalScale.x * (maxStretch * 0.5f); // Compensate X
        
        // Squash down
        float t = 0;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = new Vector3(squashX, squashY, originalScale.z);
        
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed * 2f;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Return to normal
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        isSquashing = false;
    }
}
