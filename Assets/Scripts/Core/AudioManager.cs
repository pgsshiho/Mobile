using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Source")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("UI Reference")]
    public Slider masterVolumeSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Settings")]
    public AudioMixer audioMixer;
    public AudioClip battleBgm;
    public AudioClip mainBgm;

    // 현재 볼륨 값
    public float nowMasterVolume = 1f;
    public float nowBGMVolume = 1f;
    public float nowSFXVolume = 1f;

    // 음소거 해제 시 되돌릴 이전 볼륨 값
    private float prevMasterVolume = 1f;
    private float prevBGMVolume = 1f;
    private float prevSFXVolume = 1f;

    public AudioClip moveSound;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        GlobalSaveData settings = Save.GetGlobalSaveData();
        nowMasterVolume = settings.masterVolume;
        nowBGMVolume = settings.bgmVolume;
        nowSFXVolume = settings.sfxVolume;

        // 복원용 초기값 세팅 (0 미만이면 기본 1로 지정)
        prevMasterVolume = nowMasterVolume > 0.001f ? nowMasterVolume : 1f;
        prevBGMVolume = nowBGMVolume > 0.001f ? nowBGMVolume : 1f;
        prevSFXVolume = nowSFXVolume > 0.001f ? nowSFXVolume : 1f;

        if (masterVolumeSlider != null) masterVolumeSlider.value = nowMasterVolume;
        if (bgmSlider != null) bgmSlider.value = nowBGMVolume;
        if (sfxSlider != null) sfxSlider.value = nowSFXVolume;

        SetMaster(nowMasterVolume);
        SetBGM(nowBGMVolume);
        SetSFX(nowSFXVolume);

    }
    public void Start()
    {
        PlayMainBgm();
    }
    public void SetMaster(float value)
    {
        nowMasterVolume = value;
        if (value > 0.0001f) prevMasterVolume = value; // 0이 아닐 때만 이전 값 저장

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("Master", dB);
        SaveAudioSetting(data => data.masterVolume = value);
    }

    public void SetBGM(float value)
    {
        nowBGMVolume = value;
        if (value > 0.0001f) prevBGMVolume = value;

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("BGM", dB);
        SaveAudioSetting(data => data.bgmVolume = value);
    }

    public void SetSFX(float value)
    {
        nowSFXVolume = value;
        if (value > 0.0001f) prevSFXVolume = value;

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("SFX", dB);
        SaveAudioSetting(data => data.sfxVolume = value);
    }

    // 토글 처리 (0: Master, 1: BGM, 2: SFX) - 반환값: 현재 음소거 상태 여부 (true = Muted)
    public bool ToggleMute(int type)
    {
        switch (type)
        {
            case 0: // Master
                if (nowMasterVolume > 0.0001f) { SetMaster(0.0001f); if (masterVolumeSlider) masterVolumeSlider.value = 0.0001f; return true; }
                else { SetMaster(prevMasterVolume); if (masterVolumeSlider) masterVolumeSlider.value = prevMasterVolume; return false; }

            case 1: // BGM
                if (nowBGMVolume > 0.0001f) { SetBGM(0.0001f); if (bgmSlider) bgmSlider.value = 0.0001f; return true; }
                else { SetBGM(prevBGMVolume); if (bgmSlider) bgmSlider.value = prevBGMVolume; return false; }

            case 2: // SFX
                if (nowSFXVolume > 0.0001f) { SetSFX(0.0001f); if (sfxSlider) sfxSlider.value = 0.0001f; return true; }
                else { SetSFX(prevSFXVolume); if (sfxSlider) sfxSlider.value = prevSFXVolume; return false; }
        }
        return false;
    }

    public void PlayBattleBgm()
    {
        if (bgmSource == null || battleBgm == null) return;
        if (bgmSource.clip == battleBgm && bgmSource.isPlaying) return;

        bgmSource.clip = battleBgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }
    public void PlayMainBgm()
    {
        if (bgmSource == null || battleBgm == null) return;
        if (bgmSource.clip == mainBgm && bgmSource.isPlaying) return;

        bgmSource.clip = mainBgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBattleBgm() { if (bgmSource != null) bgmSource.Stop(); }
    public void StopMainBgm() { if (bgmSource != null) bgmSource.Stop(); }
    public void PlaySfx(AudioClip clip) { if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip); }

    private static void SaveAudioSetting(System.Action<GlobalSaveData> apply)
    {
        GlobalSaveData data = Save.GetGlobalSaveData();
        apply(data);
        Save.UpdateGlobalSettings(
            data.language,
            data.masterVolume,
            data.bgmVolume,
            data.sfxVolume);
    }

    public void OnChild()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);

        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없음");
            return;
        }

        canvas.gameObject.SetActive(true);
        canvas.enabled = true;
    }

    public void OffChild()
    {
        Debug.Log("OffChild 실행");

        Canvas canvas = GetComponentInChildren<Canvas>(true);

        if (canvas == null)
        {
            Debug.LogError($"{gameObject.name}에서 Canvas를 찾지 못함");
            return;
        }

        canvas.enabled = false;
    }
}
