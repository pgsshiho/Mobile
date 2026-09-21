using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 대화가 끝난 뒤 플레이어에게 제시할 선택지 데이터다.
/// Inspector의 On Selected에 상점 열기, 퀘스트 진행 같은 결과를 연결할 수 있다.
/// </summary>
[Serializable]
public class DialogueChoice
{
    [TextArea]
    public string text;

    public UnityEvent onSelected;
}
