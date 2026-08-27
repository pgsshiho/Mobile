using UnityEngine;
using UnityEngine.UI;

public class AudioMuteButton : MonoBehaviour
{
    public enum AudioType { Master = 0, BGM = 1, SFX = 2 }

    [Header("Settings")]
    public AudioType audioType;

    [Header("UI Reference")]
    public Image iconImage;       // 변경시킬 Image 컴포넌트
    public Sprite normalIcon;     // 일반 스피커 아이콘
    public Sprite muteIcon;       // 음소거 스피커 아이콘

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClickMuteButton);
        }

        // 시작 시 현재 볼륨 상태에 맞춰 아이콘 동기화
        UpdateIconState();
    }

    public void OnClickMuteButton()
    {
        if (AudioManager.instance == null) return;

        // AudioManager에 토글 요청 후, 결과 상태(Mute 여부) 받아오기
        bool isMuted = AudioManager.instance.ToggleMute((int)audioType);

        // 아이콘 교체
        if (iconImage != null)
        {
            iconImage.sprite = isMuted ? muteIcon : normalIcon;
        }
    }

    private void UpdateIconState()
    {
        if (AudioManager.instance == null || iconImage == null) return;

        float currentVol = 0f;
        switch (audioType)
        {
            case AudioType.Master: currentVol = AudioManager.instance.nowMasterVolume; break;
            case AudioType.BGM: currentVol = AudioManager.instance.nowBGMVolume; break;
            case AudioType.SFX: currentVol = AudioManager.instance.nowSFXVolume; break;
        }

        // 볼륨이 최저 수준이면 Mute 아이콘으로 설정
        iconImage.sprite = (currentVol <= 0.0001f) ? muteIcon : normalIcon;
    }
}