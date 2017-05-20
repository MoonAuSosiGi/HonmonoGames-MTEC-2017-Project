using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoboHUD : MonoBehaviour, GameUI.HPUpdateEvent
{
    List<Image> m_hpList = new List<Image>();
    public TextMesh m_roboUI = null;

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

        m_roboUI.text = "HP : " + curHP + "\nENERGY\n100%";
    }
}
