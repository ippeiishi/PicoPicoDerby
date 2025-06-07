using UnityEngine;
using System.Threading;
using UnityEngine.UI;
public class AudioManager : MonoBehaviour
{
    public static AudioManager i;

    public AudioSource bgmSource;
    public AudioSource seSource;

    public AudioClip[] bgmClips;
    public AudioClip[] seClips;
    public AudioClip[] jingleClips;

    public AudioClip okSe; // 決定音
    public AudioClip cancelSe;   // キャンセル音
    public AudioClip clickSe;   // キャンセル音
    public AudioClip slideoutSe;   // キャンセル音
    private bool isMuted = false;
    private const string MutedKey = "Muted";

public GameObject MuteButton;

[SerializeField] private Slider bgmSlider;
[SerializeField] private Slider seSlider;

private void Start(){
    if (bgmSlider != null) bgmSlider.value = bgmSource.volume;
    if (seSlider != null) seSlider.value = seSource.volume;
}
public void SetBGMVolume(float volume){
    bgmSource.volume = volume;
}

public void SetSEVolume(float volume){
    seSource.volume = volume;
}
    private void Awake()
    {
        if (i == null)
        {
            i = this;
            // シーンを跨がない場合は DontDestroyOnLoad は不要です
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ミュート状態の読み込み
        isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        UpdateMuteState();
        PlayBGM(0, 1.0f);
    }

    public void PlayBGM(int index, float volume = 1.0f)
    {
        if (IsValidClip(bgmClips, index))
        {
            bgmSource.clip = bgmClips[index];
            bgmSource.volume = volume;
            bgmSource.Play();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void UnPauseBGM()
    {
        if (bgmSource.clip != null)
        {
            bgmSource.UnPause();
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
public void PlaySE(int index, float volume = 1.0f, float delay = 0.0f)
{
    if (IsValidClip(seClips, index))
    {
        AudioClip clip = seClips[index];

        // 新しい AudioSource を生成
        GameObject tempAudio = new GameObject("TempAudioSource");
        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();

        tempSource.clip = clip;
        tempSource.volume = volume;

        // ミュート状態を適用
        tempSource.mute = isMuted;

        // 再生開始位置を設定
        if (delay > 0.0f)
        {
            tempSource.time = delay;
        }

        tempSource.Play();

        // 再生終了後に削除
        Destroy(tempAudio, clip.length - delay);
    }
}





    public void PlayJingle(int index, float volume = 1.0f)
    {
        if (IsValidClip(jingleClips, index))
        {
            bgmSource.Stop();
            bgmSource.clip = jingleClips[index];
            bgmSource.volume = volume;
            bgmSource.Play();
        }
    }

    public void PlayClickSe(){
            seSource.PlayOneShot(clickSe);
    }
   public void PlayOKSe(){
            seSource.PlayOneShot(okSe);
    }
    public void PlayCancelSe()
    {
            seSource.PlayOneShot(cancelSe);
    }
    public void PlayslideoutSe(){
            seSource.PlayOneShot(slideoutSe);
    }
    public void ToggleMute()
    {
        isMuted = !isMuted;
        UpdateMuteState();
        PlayerPrefs.SetInt(MutedKey, isMuted ? 1 : 0);
    }

    private void UpdateMuteState()
    {
        // BGMとSEのミュート状態を適用
        bgmSource.mute = isMuted;
        seSource.mute = isMuted;

        // MuteButton の子オブジェクトの表示を切り替え
        if (MuteButton != null && MuteButton.transform.childCount >= 2)
        {
            MuteButton.transform.GetChild(0).gameObject.SetActive(!isMuted); // ミュートOFF時（表示）
            MuteButton.transform.GetChild(1).gameObject.SetActive(isMuted);  // ミュートON時（表示）
        }
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    private bool IsValidClip(AudioClip[] clips, int index)
    {
        return clips != null && index >= 0 && index < clips.Length && clips[index] != null;
    }
}
