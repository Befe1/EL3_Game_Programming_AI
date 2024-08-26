using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_manager : MonoBehaviour
{
    public static s_manager Instance { get; private set; }

    private Dictionary<string, AudioSource> audioSources;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        public bool loop;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
    }

    [SerializeField] private Sound[] sounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSources = new Dictionary<string, AudioSource>();

        foreach (Sound sound in sounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = sound.clip;
            source.loop = sound.loop;
            source.volume = sound.volume;
            source.pitch = sound.pitch;
            audioSources[sound.name] = source;
        }
    }

    public void PlaySound(string name)
    {
        if (audioSources.ContainsKey(name))
        {
            audioSources[name].Play();
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found!");
        }
    }
    public AudioClip GetClip(string name)
    {
        if (audioSources.ContainsKey(name))
        {
            return audioSources[name].clip;
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found!");
            return null;
        }
    }

    public void StopSound(string name)
    {
        if (audioSources.ContainsKey(name))
        {
            audioSources[name].Stop();
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found!");
        }
    }

    public void SetVolume(string name, float volume)
    {
        if (audioSources.ContainsKey(name))
        {
            audioSources[name].volume = volume;
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found!");
        }
    }

    public void SetPitch(string name, float pitch)
    {
        if (audioSources.ContainsKey(name))
        {
            audioSources[name].pitch = pitch;
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found!");
        }
    }
}
