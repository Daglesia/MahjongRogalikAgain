using UnityEngine;

/// Background music. Lives in MenuScene, survives scene loads (Singleton -> DontDestroyOnLoad)
/// and loops. If MenuScene is loaded again, the duplicate destroys itself and the music keeps playing.
[RequireComponent(typeof(AudioSource))]
public class MusicManager : Singleton<MusicManager>
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [Tooltip("On = music keeps playing while the pause menu is open (AudioListener.pause is ignored).")]
    [SerializeField] private bool playWhilePaused = true;

    private AudioSource source;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;   // duplicate, about to be destroyed

        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = playWhilePaused;
        source.volume = volume;

        Play(musicClip);
    }

    /// Starts a track (looping). Does nothing if that track is already playing.
    public void Play(AudioClip clip)
    {
        if (clip == null || source == null) return;
        if (source.clip == clip && source.isPlaying) return;

        source.clip = clip;
        source.Play();
    }

    public void Stop()
    {
        if (source != null) source.Stop();
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        if (source != null) source.volume = volume;
    }
}