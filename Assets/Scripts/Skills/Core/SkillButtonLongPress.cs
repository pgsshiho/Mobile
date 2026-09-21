using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SkillButtonLongPress :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Long Press")]
    [Tooltip("이 시간 이상 누르면 길게 누르기로 판단합니다.")]
    public float holdTime = 0.5f;

    private int skillIndex;

    private Coroutine holdCoroutine;

    private bool isPointerDown;
    private bool longPressed;


    public void Initialize(int index)
    {
        skillIndex = index;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (BattleManager.instance == null)
            return;

        if (TurnManager.instance == null)
            return;

        if (!(TurnManager.instance.currentUnit is PlayerUnit))
            return;

        if (!TurnManager.instance.waitingForTarget)
            return;

        isPointerDown = true;
        longPressed = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }

        holdCoroutine = StartCoroutine(LongPressCheck());
    }


    private IEnumerator LongPressCheck()
    {
        yield return new WaitForSeconds(holdTime);

        if (!isPointerDown)
            yield break;

        longPressed = true;

        // 길게 누르면 설명창 표시
        BattleManager.instance.ShowBattleUIEX(skillIndex);
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown)
            return;

        isPointerDown = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        if (longPressed)
        {
            // 길게 누른 경우
            // 손을 떼면 EX UI 닫기
            if (BattleManager.instance != null)
            {
                BattleManager.instance.HideBattleUIEX();
            }

            longPressed = false;

            return;
        }

        // 짧게 누른 경우에만 스킬 선택
        if (BattleManager.instance != null)
        {
            BattleManager.instance.SelectSkill(skillIndex);
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPointerDown)
            return;

        isPointerDown = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        // 길게 누르기 전에 버튼 밖으로 나간 경우
        if (!longPressed)
        {
            return;
        }

        // 롱프레스 상태에서 버튼 밖으로 나간 경우에도 닫기
        if (BattleManager.instance != null)
        {
            BattleManager.instance.HideBattleUIEX();
        }

        longPressed = false;
    }


    private void OnDisable()
    {
        isPointerDown = false;
        longPressed = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }
}