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
        instance = this;

        nowMasterVolume = PlayerPrefs.GetFloat("Master", 1f);
        nowBGMVolume = PlayerPrefs.GetFloat("BGM", 1f);
        nowSFXVolume = PlayerPrefs.GetFloat("SFX", 1f);

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

    public void SetMaster(float value)
    {
        nowMasterVolume = value;
        if (value > 0.0001f) prevMasterVolume = value; // 0이 아닐 때만 이전 값 저장

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("Master", dB);
        PlayerPrefs.SetFloat("Master", value);
    }

    public void SetBGM(float value)
    {
        nowBGMVolume = value;
        if (value > 0.0001f) prevBGMVolume = value;

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("BGM", dB);
        PlayerPrefs.SetFloat("BGM", value);
    }

    public void SetSFX(float value)
    {
        nowSFXVolume = value;
        if (value > 0.0001f) prevSFXVolume = value;

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("SFX", dB);
        PlayerPrefs.SetFloat("SFX", value);
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

    public void StopBgm() { if (bgmSource != null) bgmSource.Stop(); }
    public void PlaySfx(AudioClip clip) { if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip); }
}