using System;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    TripletPlayed,
    SequencePlayed
}

[RequireComponent(typeof(AudioSource))]
public class AudioManager : CardColleague
{
    [Serializable]
    public class SoundEntry
    {
        public SoundType type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

    private AudioSource source;
    private readonly Dictionary<SoundType, SoundEntry> lookup = new Dictionary<SoundType, SoundEntry>();

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;

        foreach (SoundEntry entry in sounds)
            lookup[entry.type] = entry;
    }

    public void PlaySound(SoundType type)
    {
        if (!lookup.TryGetValue(type, out SoundEntry entry) || entry.clip == null)
        {
            Debug.LogWarning($"AudioManager: no clip assigned for {type}");
            return;
        }

        source.PlayOneShot(entry.clip, entry.volume);
    }
}
