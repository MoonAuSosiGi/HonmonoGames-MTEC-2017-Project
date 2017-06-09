using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoboHUD : MonoBehaviour, GameUI.RobotHPUpdateEvent, GameUI.ENERGYUpdateEvent , GameUI.CharacterHPUpdateEvent
{
    //-- 정보 ----------------------------------------------------------//
    // 상단 적 hp 전용
    public Image m_topEnemyHPBar = null;
    
    // 왼쪽 캐릭터 hp 전용
    public Image m_characterHP = null;
    public List<Sprite> m_charHPList = new List<Sprite>();
    // 오른쪽 로봇 hp 전용
    public Image m_roboHPGaugeBar = null;
    public Text m_roboHPText = null;
    // 아래쪽 로봇 energy 전용
    public List<Sprite> m_energyGaugeBar = new List<Sprite>();
    public List<Image> m_energyList = new List<Image>();
    public Text m_energyPercent = null;

    //-----------------------------------------------------------------//

    public TextMesh m_roboUI = null;
    
    private float m_energy = 0.0f;

    void Start()
    {
        Transform energy = transform.GetChild(0).GetChild(0);

        for (int i = 0; i < energy.childCount; i++)
            m_energyList.Add(energy.GetChild(i).GetComponent<Image>());
    }


    // 캐릭터 HP
    void GameUI.CharacterHPUpdateEvent.HPUpdate(int curHP , int maxHP)
    {
        if (maxHP == 0)
            return;
        int percent = (curHP / maxHP);

        Sprite spr = m_characterHP.sprite;
        // 오름차순 이미지로 가정
        for (int i = 0; i < m_charHPList.Count; i++)
        {
            if (percent <= i * 10)
                spr = m_charHPList[i];
        }
        m_characterHP.sprite = spr;

        UpdateUI();
    }

    void GameUI.RobotHPUpdateEvent.HPUpdate(int curHP,int maxHP)
    {
        if (maxHP == 0)
            return;
        int percent = (curHP / maxHP);
        float width = m_roboHPGaugeBar.sprite.rect.size.x;
        RectTransform t = m_roboHPGaugeBar.GetComponent<RectTransform>();
        t.sizeDelta = new Vector2(width * percent , t.sizeDelta.y);
        m_roboHPText.text = string.Format("{0:F1}%" , (percent * 100.0f));

    }

    void GameUI.ENERGYUpdateEvent.EnergyUpdate(float curEnergy)
    {
        float bar_percent = 100.0f / m_energyList.Count;

        int barIndex = m_energyGaugeBar.Count - 1;
        for(int i = m_energyList.Count; i >= 0; i--)
        {
            if(curEnergy >= bar_percent * i)
            {
                m_energyList[i].gameObject.SetActive(true);
                m_energyList[i].sprite = m_energyGaugeBar[barIndex--];
                if (barIndex < 0)
                    barIndex = 0;
            }
            else
                m_energyList[i].gameObject.SetActive(false);
        }

        m_energy = curEnergy;
        m_energyPercent.text = string.Format("{0:F1}%" , Mathf.Round(curEnergy));
        UpdateUI();
    }

    void UpdateUI()
    {
        if (GameManager.Instance().ROBO == null)
            return;

        m_roboUI.text = "HP : " + GameManager.Instance().ROBO.HP + "\nENERGY\n " + string.Format("{0:F1}%" , GameManager.Instance().ROBO.ENERGY);
    }

   
}
