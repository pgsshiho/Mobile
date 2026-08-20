using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public int money;

    private void Awake()
    {
        Instance = this;
    }

    // 방어 강화
    public void BuyDefenseUpgrade(Unit user)
    {
        if (user.defenseLevel >=
            user.maxDefenseLevel)
        {
            Debug.Log("최대 레벨");
            return;
        }

        int price =
            50 * (user.defenseLevel + 1);

        if (money < price)
        {
            Debug.Log("돈 부족");
            return;
        }

        money -= price;

        user.defenseLevel++;

        user.defensePower += 3;

        Debug.Log(
            user.name +
            " 방어 강화 Lv." +
            user.defenseLevel
        );
    }

    // 공격 강화
    public void BuyAttackUpgrade(Unit user)
    {
        if (user.attackLevel >=
            user.maxAttackLevel)
        {
            Debug.Log("최대 레벨");
            return;
        }

        int price =
            100 * (user.attackLevel + 1);

        if (money < price)
        {
            Debug.Log("돈 부족");
            return;
        }

        money -= price;

        user.attackLevel++;

        user.attackPower += 5;

        Debug.Log(
            user.name +
            " 공격 강화 Lv." +
            user.attackLevel
        );
    }

    // 정확도 강화
    public void BuyAccuracyUpgrade(Unit user)
    {
        if (user.accuracyupgrade)
        {
            Debug.Log("최대 레벨");
            return;
        }

        if (money < 3000)
        {
            Debug.Log("돈 부족");
            return;
        }

        money -= 3000;

        user.accuracyupgrade = true;

        user.accuracy += 10;

        Debug.Log(
            user.name +
            " 정확도 강화 완료"
        );
    }
}