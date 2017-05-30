using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoboHUD : MonoBehaviour, GameUI.HPUpdateEvent, GameUI.ENERGYUpdateEvent
{
    List<Image> m_hpList = new List<Image>();
    public TextMesh m_roboUI = null;
    private int m_hp = 0;
    private float m_energy = 0.0f;

    void Start()
    {
        // 0 번 차일드가 이미지 리스트
        Transform hplist = transform.GetChild(0);
        for (int i = 0; i < hplist.childCount; i++)
            m_hpList.Add(hplist.GetChild(i).GetComponent<Image>());
    }
    

    void GameUI.HPUpdateEvent.HPUpdate(int curHP,int maxHP)
    {
        int imageHP = maxHP / m_hpList.Count; // 100 / 5  = 20
        int curImageHP = curHP / imageHP; // 100 / 20 = 5     99 / 20 = 4
   
        for (int i = 0; i < m_hpList.Count; i++)
        {
            if (i < curImageHP)
                m_hpList[i].gameObject.SetActive(true);
            else
                m_hpList[i].gameObject.SetActive(false);
        }

        UpdateUI();
    }

    void GameUI.ENERGYUpdateEvent.EnergyUpdate(float curEnergy)
    {
        m_energy = curEnergy;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (GameManager.Instance().ROBO == null)
            return;

        m_roboUI.text = "HP : " + GameManager.Instance().ROBO.HP + "\nENERGY\n " + string.Format("{0:F1}%" , GameManager.Instance().ROBO.ENERGY);
    }
}
