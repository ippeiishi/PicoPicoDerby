using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    public AudioSource bgmSource;
    public AudioClip[] bgmClips;
    public AudioClip[] jingleClips;

    [System.Serializable]
    public struct SoundEffect {
        public string name;
        public AudioClip clip;
    }

    [Header("Sound Effects")]
    [SerializeField] private List<SoundEffect> soundEffects;
    private Dictionary<string, AudioClip> seDictionary;

    [SerializeField] private int seSourcePoolSize = 8;
    private List<AudioSource> seSourcePool;
    private int nextSeSourceIndex = 0;

    private float bgmBaseVolume = 1.0f;
    private float seBaseVolume = 1.0f;

    private const string BGMVolumeKey = "BGMVolume";
    private const string SEVolumeKey = "SEVolume";

    private void Awake() {
        Instance = this;
        seDictionary = soundEffects.ToDictionary(se => se.name, se => se.clip);
    }

    private void Start() {
        seSourcePool = new List<AudioSource>();
        for (int j = 0; j < seSourcePoolSize; j++) {
            GameObject sourceGO = new GameObject("SE_Source_" + j);
            sourceGO.transform.SetParent(this.transform);
            AudioSource source = sourceGO.AddComponent<AudioSource>();
            source.playOnAwake = false;
            seSourcePool.Add(source);
        }

        bgmBaseVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 1.0f);
        seBaseVolume = PlayerPrefs.GetFloat(SEVolumeKey, 1.0f);

        SetBGMVolume(bgmBaseVolume);
        SetSEVolume(seBaseVolume);

        PlayBGM(0);
    }

    public void PlaySE(string soundName) {
        if (seDictionary.TryGetValue(soundName, out AudioClip clip)) {
            PlayOneShot(clip);
        } else {
            Debug.LogWarning($"[AudioManager] Sound effect '{soundName}' not found.");
        }
    }

    public void SetBGMVolume(float volume) {
        bgmBaseVolume = volume;
        bgmSource.volume = bgmBaseVolume;
        PlayerPrefs.SetFloat(BGMVolumeKey, bgmBaseVolume);
    }

    public void SetSEVolume(float volume) {
        seBaseVolume = volume;
        foreach (var source in seSourcePool) {
            source.volume = seBaseVolume;
        }
        PlayerPrefs.SetFloat(SEVolumeKey, seBaseVolume);
    }

    public void PlayBGM(int index, float volumeScale = 1.0f) {
        if (IsValidClip(bgmClips, index)) {
            bgmSource.clip = bgmClips[index];
            bgmSource.volume = bgmBaseVolume * volumeScale;
            bgmSource.Play();
        }
    }

    public void PlayJingle(int index, float volumeScale = 1.0f) {
        if (IsValidClip(jingleClips, index)) {
            bgmSource.Stop();
            bgmSource.clip = jingleClips[index];
            bgmSource.volume = bgmBaseVolume * volumeScale;
            bgmSource.Play();
        }
    }

    private void PlayOneShot(AudioClip clip) {
        if (clip == null) { return; }
        
        AudioSource source = seSourcePool[nextSeSourceIndex];
        nextSeSourceIndex = (nextSeSourceIndex + 1) % seSourcePoolSize;
        
        source.PlayOneShot(clip, seBaseVolume);
    }

    private bool IsValidClip(AudioClip[] clips, int index) {
        return clips != null && index >= 0 && index < clips.Length && clips[index] != null;
    }
}