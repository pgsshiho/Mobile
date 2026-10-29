using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatSkillUI : MonoBehaviour
{
    PlayerUnit pu;
    SpriteRenderer sr;
    public List<SkillData> skills = new List<SkillData>();
    public List<Image> skillButtons = new List<Image>();
    SkillData skill;
    private void Awake()
    {
        pu = GetComponent<PlayerUnit>();
        sr = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        GetSkillimage();
    }

    // Update is called once per frame

    public void GetSkillimage()
    {
        for(int i = 0; i < 6; i++)
        {
            skills.Add(pu.skills[i]);
        }

        for(int i = 0; i < skillButtons.Count; i++)
        {
            skillButtons[i].sprite = skills[i].icon;
        }
    }
}
