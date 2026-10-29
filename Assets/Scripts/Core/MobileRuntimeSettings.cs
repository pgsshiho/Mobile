using UnityEngine;

/// <summary>
/// 씬에 배치하지 않아도 적용되는 모바일 런타임 기본값입니다.
/// 기기 성능별 품질 설정은 Unity Quality Settings에서 별도로 관리합니다.
/// </summary>
public static class MobileRuntimeSettings
{
    private const int TargetFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
#if UNITY_ANDROID || UNITY_IOS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
    }
}
