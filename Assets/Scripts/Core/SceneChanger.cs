using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 전환에 필요
using DG.Tweening;

public class SceneChanger : MonoBehaviour
{
    private static Image image;
    public static bool isFading = false;

    private void Awake()
    {
        image = GetComponent<Image>();

        isFading = true;
        StartCoroutine(wait(0.2f, () =>
        {
            image.DOColor(Color.clear, 0.7f).OnComplete(() =>
            {
                isFading = false;
                gameObject.SetActive(false);
            });
        }));
    }

    public static void BG(string sceneName)
    {
        // 연동된 Image가 없거나 이미 페이드 중이면 중복 실행 방지
        if (image == null || isFading) return;

        image.gameObject.SetActive(true);
        isFading = true;

        // 화면을 검게 만든 뒤(0.7초) 씬을 실제 이동시킵니다.
        image.DOColor(Color.black, 0.7f).OnComplete(() =>
        {
            isFading = false;
            SceneManager.LoadScene(sceneName); // ★ 이 코드가 누락되어 있었습니다!
        });
    }

    public static IEnumerator wait(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action?.Invoke();
    }
}