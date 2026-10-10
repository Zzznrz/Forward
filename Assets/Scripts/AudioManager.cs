using UnityEngine;

/// <summary>
/// Lightweight procedural audio for the playable submission build.
/// It keeps the project self-contained when external sound assets are unavailable.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private const int SampleRate = 44100;
    private static AudioManager instance;

    private AudioSource effectsSource;
    private AudioSource musicSource;
    private AudioClip jumpClip;
    private AudioClip slideClip;
    private AudioClip deathClip;
    private AudioClip finishClip;
    private AudioClip clickClip;
    private AudioClip musicClip;

    public static AudioManager GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject audioObject = new GameObject("AudioManager");
        instance = audioObject.AddComponent<AudioManager>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        effectsSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.12f;

        CreateClips();
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void PlayJump() => PlayEffect(jumpClip, 0.55f);
    public void PlaySlide() => PlayEffect(slideClip, 0.35f);
    public void PlayDeath() => PlayEffect(deathClip, 0.7f);
    public void PlayFinish() => PlayEffect(finishClip, 0.65f);
    public void PlayClick() => PlayEffect(clickClip, 0.25f);

    private void PlayEffect(AudioClip clip, float volume)
    {
        if (clip != null && effectsSource != null)
            effectsSource.PlayOneShot(clip, volume);
    }

    private void CreateClips()
    {
        jumpClip = CreateTone("Jump", 0.14f, 480f, 900f);
        slideClip = CreateTone("Slide", 0.16f, 220f, 90f);
        deathClip = CreateTone("Death", 0.34f, 180f, 55f, true);
        finishClip = CreateChord("Finish", 0.55f, new[] { 523f, 659f, 784f });
        clickClip = CreateTone("Click", 0.045f, 720f, 560f);
        musicClip = CreateMusicLoop();
    }

    private static AudioClip CreateTone(string clipName, float duration, float startFrequency, float endFrequency, bool rough = false)
    {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];
        for (int index = 0; index < sampleCount; index++)
        {
            float progress = index / (float)sampleCount;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
            float envelope = 1f - progress;
            float tone = Mathf.Sin(2f * Mathf.PI * frequency * index / SampleRate);
            if (rough)
                tone = Mathf.Sign(tone) * 0.55f + tone * 0.45f;
            samples[index] = tone * envelope * 0.5f;
        }
        return CreateClip(clipName, samples);
    }

    private static AudioClip CreateChord(string clipName, float duration, float[] frequencies)
    {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];
        for (int index = 0; index < sampleCount; index++)
        {
            float progress = index / (float)sampleCount;
            float value = 0f;
            foreach (float frequency in frequencies)
                value += Mathf.Sin(2f * Mathf.PI * frequency * index / SampleRate);
            samples[index] = value / frequencies.Length * (1f - progress) * 0.45f;
        }
        return CreateClip(clipName, samples);
    }

    private static AudioClip CreateMusicLoop()
    {
        const float duration = 8f;
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];
        float[] notes = { 110f, 138.59f, 164.81f, 138.59f };
        for (int index = 0; index < sampleCount; index++)
        {
            float time = index / (float)SampleRate;
            float note = notes[Mathf.FloorToInt(time * 0.5f) % notes.Length];
            float pulse = Mathf.Sin(2f * Mathf.PI * note * time) * 0.22f;
            float pad = Mathf.Sin(2f * Mathf.PI * note * 2f * time) * 0.08f;
            samples[index] = pulse + pad;
        }
        return CreateClip("ForwardLoop", samples);
    }

    private static AudioClip CreateClip(string clipName, float[] samples)
    {
        AudioClip clip = AudioClip.Create(clipName, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
