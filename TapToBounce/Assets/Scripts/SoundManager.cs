using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource audioSource;
    private AudioClip jumpClip;
    private AudioClip dieClip;
    private AudioClip coinClip;
    private AudioClip shieldClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            GenerateSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void GenerateSounds()
    {
        jumpClip = CreateTone(600, 0.1f, true);
        dieClip = CreateNoise(0.3f);
        coinClip = CreateTone(1200, 0.1f, false);
        shieldClip = CreateTone(300, 0.3f, false);
    }

    public void PlayJump() { audioSource.PlayOneShot(jumpClip, 0.5f); }
    public void PlayDie() { audioSource.PlayOneShot(dieClip, 1.0f); }
    public void PlayCoin() { audioSource.PlayOneShot(coinClip, 0.7f); }
    public void PlayShield() { audioSource.PlayOneShot(shieldClip, 0.8f); }

    AudioClip CreateTone(float frequency, float duration, bool slide)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = slide ? Mathf.Lerp(frequency, frequency * 2, t) : frequency;
            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate);
        }
        AudioClip clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateNoise(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = (Random.value * 2 - 1) * (1 - (float)i/sampleCount); // Decay
        }
        AudioClip clip = AudioClip.Create("Noise", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
