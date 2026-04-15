using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        image.gameObject.SetActive(true);
        isFading = true;
        image.DOColor(Color.black, 0.7f).OnComplete(() =>
        {
            isFading = false;
            SceneManager.LoadScene(sceneName);
        });
    }
    public static IEnumerator wait(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}
