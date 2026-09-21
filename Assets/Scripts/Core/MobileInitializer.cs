using UnityEngine;

/// <summary>
/// 모바일 환경을 위한 프레임레이트, VSync, 화면 절전 모드 자동 초기화기입니다.
/// 별도의 씬 컴포넌트 추가 없이 게임 시작 시 자동으로 동작합니다.
/// </summary>
public static class MobileInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeMobileSettings()
    {
        // 1. 프레임레이트 60 FPS 고정 (안정적인 프레임 페이싱 및 배터리/발열 최적화)
        Application.targetFrameRate = 60;

        // 2. 수직 동기화 비활성화 (모바일 환경에서는 targetFrameRate가 주도하도록 설정)
        QualitySettings.vSyncCount = 0;

        // 3. 게임 플레이 중 화면 자동 꺼짐(절전) 방지
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Debug.Log("<color=cyan>[MobileInitializer]</color> 모바일 최적화 세팅 완료: TargetFPS=60, VSync=0, SleepTimeout=NeverSleep");
    }
}
