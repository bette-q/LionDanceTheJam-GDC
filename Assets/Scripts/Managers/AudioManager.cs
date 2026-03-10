using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Serializable]
    private struct AudioEntry
    {
        public string id;
        public AudioClip clip;
    }

    public static AudioManager Instance { get; private set; }

    [Header("Audio Library")]
    [SerializeField] private AudioEntry[] audioEntries;
    [SerializeField] private string defaultBgmId;
    [SerializeField] private bool playDefaultBgmOnStart = true;

    [Header("Audio Output")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField, Range(0f, 1f)] private float defaultMasterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultBgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 1f;

    private readonly Dictionary<string, AudioClip> _clipById = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
    private float _masterVolume;
    private float _bgmVolume;
    private float _sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        _masterVolume = Mathf.Clamp01(defaultMasterVolume);
        _bgmVolume = Mathf.Clamp01(defaultBgmVolume);
        _sfxVolume = Mathf.Clamp01(defaultSfxVolume);
        RefreshOutputVolumes();
        BuildLookup();

        if (playDefaultBgmOnStart && !string.IsNullOrWhiteSpace(defaultBgmId))
        {
            PlayBgm(defaultBgmId, true);
        }
    }

    public bool Play(string id, bool loop = false)
    {
        return PlayBgm(id, loop);
    }

    public bool PlayBgm(string id, bool loop = true)
    {
        if (!TryGetClip(id, out AudioClip clip))
        {
            return false;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
        return true;
    }

    public void Pause()
    {
        PauseBgm();
    }

    public void PauseBgm()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void Resume()
    {
        ResumeBgm();
    }

    public void ResumeBgm()
    {
        bgmSource.UnPause();
    }

    public void Replay()
    {
        ReplayBgm();
    }

    public void ReplayBgm()
    {
        if (bgmSource.clip == null)
        {
            return;
        }

        bgmSource.Stop();
        bgmSource.Play();
    }

    public void Stop()
    {
        StopBgm();
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }

    public bool PlaySfx(string id, float volumeScale = 1f)
    {
        if (!TryGetClip(id, out AudioClip clip))
        {
            return false;
        }

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        return true;
    }

    public void PauseSfx()
    {
        sfxSource.Pause();
    }

    public void ResumeSfx()
    {
        sfxSource.UnPause();
    }

    public void StopSfx()
    {
        sfxSource.Stop();
    }

    public void SetVolume(float volume)
    {
        SetBgmVolume(volume);
    }

    public void SetBgmVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        RefreshOutputVolumes();
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        RefreshOutputVolumes();
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        RefreshOutputVolumes();
    }

    public float GetVolume()
    {
        return GetBgmVolume();
    }

    public float GetBgmVolume()
    {
        return _bgmVolume;
    }

    public float GetSfxVolume()
    {
        return _sfxVolume;
    }

    public float GetMasterVolume()
    {
        return _masterVolume;
    }

    private void BuildLookup()
    {
        _clipById.Clear();

        if (audioEntries == null)
        {
            return;
        }

        for (int i = 0; i < audioEntries.Length; i++)
        {
            AudioEntry entry = audioEntries[i];
            if (string.IsNullOrWhiteSpace(entry.id) || entry.clip == null)
            {
                continue;
            }

            _clipById[entry.id] = entry.clip;
        }
    }

    private bool TryGetClip(string id, out AudioClip clip)
    {
        clip = null;
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return _clipById.TryGetValue(id, out clip);
    }

    private void RefreshOutputVolumes()
    {
        bgmSource.volume = _bgmVolume * _masterVolume;
        sfxSource.volume = _sfxVolume * _masterVolume;
    }
}
