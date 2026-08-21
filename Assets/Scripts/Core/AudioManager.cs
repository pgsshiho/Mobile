using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Source")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM")]
    public AudioClip battleBgm;

    private void Awake()
    {
        instance = this;
    }

    public void PlayBattleBgm()
    {
        if (bgmSource == null)
            return;

        if (battleBgm == null)
            return;

        if (bgmSource.clip == battleBgm &&
            bgmSource.isPlaying)
            return;

        bgmSource.clip = battleBgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource == null)
            return;

        bgmSource.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null)
            return;

        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}