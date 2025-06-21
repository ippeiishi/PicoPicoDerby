using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
    // DI_MODEL違反を修正: i -> Instance
    public static AudioManager Instance { get; private set; }

    public AudioSource bgmSource;
    public AudioClip[] bgmClips;
    public AudioClip[] seClips;
    public AudioClip[] jingleClips;

    public AudioClip okSe;
    public AudioClip cancelSe;
    public AudioClip clickSe;
    public AudioClip slideoutSe;

    [SerializeField] private int seSourcePoolSize = 8;
    private List<AudioSource> seSourcePool;
    private int nextSeSourceIndex = 0;

    private float bgmBaseVolume = 1.0f;
    private float seBaseVolume = 1.0f;

    private const string BGMVolumeKey = "BGMVolume";
    private const string SEVolumeKey = "SEVolume";

    // DI_MODEL違反とCODING_STYLEを修正
    private void Awake() { Instance = this; }

    // CODING_STYLEを修正
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

    // CODING_STYLEを修正
    public void SetBGMVolume(float volume) {
        bgmBaseVolume = volume;
        bgmSource.volume = bgmBaseVolume;
        PlayerPrefs.SetFloat(BGMVolumeKey, bgmBaseVolume);
    }

    // CODING_STYLEを修正
    public void SetSEVolume(float volume) {
        seBaseVolume = volume;
        foreach (var source in seSourcePool) {
            source.volume = seBaseVolume;
        }
        PlayerPrefs.SetFloat(SEVolumeKey, seBaseVolume);
    }

    // CODING_STYLEを修正
    public void PlayBGM(int index, float volumeScale = 1.0f) {
        if (IsValidClip(bgmClips, index)) {
            bgmSource.clip = bgmClips[index];
            bgmSource.volume = bgmBaseVolume * volumeScale;
            bgmSource.Play();
        }
    }

    // CODING_STYLEを修正
    public void PlaySE(int index, float volumeScale = 1.0f, float delay = 0.0f) {
        if (IsValidClip(seClips, index)) {
            AudioSource source = seSourcePool[nextSeSourceIndex];
            nextSeSourceIndex = (nextSeSourceIndex + 1) % seSourcePoolSize;

            source.clip = seClips[index];
            source.volume = seBaseVolume * volumeScale;
            
            if (delay > 0.0f) {
                source.PlayDelayed(delay);
            } else {
                source.Play();
            }
        }
    }

    // CODING_STYLEを修正
    public void PlayJingle(int index, float volumeScale = 1.0f) {
        if (IsValidClip(jingleClips, index)) {
            bgmSource.Stop();
            bgmSource.clip = jingleClips[index];
            bgmSource.volume = bgmBaseVolume * volumeScale;
            bgmSource.Play();
        }
    }

    // CODING_STYLEを修正
    private void PlayOneShot(AudioClip clip) {
        if (clip == null) { return; }
        
        AudioSource source = seSourcePool[nextSeSourceIndex];
        nextSeSourceIndex = (nextSeSourceIndex + 1) % seSourcePoolSize;
        
        source.PlayOneShot(clip, seBaseVolume);
    }

    // CODING_STYLE (単一ステートメントのメソッド) を適用
    public void PlayClickSe() { PlayOneShot(clickSe); }
    public void PlayOKSe() { PlayOneShot(okSe); }
    public void PlayCancelSe() { PlayOneShot(cancelSe); }
    public void PlayslideoutSe() { PlayOneShot(slideoutSe); }

    // CODING_STYLE (単一ステートメントのメソッド) を適用
    private bool IsValidClip(AudioClip[] clips, int index) {
        return clips != null && index >= 0 && index < clips.Length && clips[index] != null;
    }
}