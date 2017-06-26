using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCharger : MonoBehaviour ,NetworkManager.NetworkMessageEventListenrer,GameUI.ENERGYUpdateEvent{

    public GameObject m_EnergyBar = null;
    bool m_networkCheck = false;

   
    // -- 에너지 차지 ---------------------------------------------------------------------------------------------------------//
    public SpriteRenderer m_chargePad = null;
    public GameObject m_chargeTopObj = null;
    public GameObject m_chargeBottomObj = null;

    void Start()
    {
        float distance = m_EnergyBar.transform.GetChild(1).position.y - m_EnergyBar.transform.GetChild(0).position.y;

        for(int i = 2; i < m_EnergyBar.transform.childCount; i++)
        {
            Transform t = m_EnergyBar.transform.GetChild(i);
            Vector3 prevPos = m_EnergyBar.transform.GetChild(i - 1).position;
            t.position = new Vector3(t.position.x , prevPos.y + distance , t.position.z);
        }
        UpdateEnergy();
        GameManager.Instance().AddEnergyUpdateEvent(this);
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
    }

    void Update()
    {
        // Network 체크
        if(!m_networkCheck && GameManager.Instance().PLAYER != null)
        {
            if (!string.IsNullOrEmpty(NetworkOrderController.ORDER_NAME) &&
                !NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
            {
                
                NetworkMoving m = transform.GetChild(1).gameObject.AddComponent<NetworkMoving>();
                m.NAME = NetworkOrderController.ORDER_NAME + "_pad";
                m_networkCheck = true;
            }
        }
    }

    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if (e.msgType.Equals(NetworkManager.ENERGY_UPDATE))
        {
            
            if (!e.targetName.Equals("robo"))
                return;

            GameManager.Instance().ROBO.ENERGY = e.msg.GetField(NetworkManager.ENERGY_UPDATE).f;
            UpdateEnergy();
        }
    }

    void UpdateEnergy()
    {
        if (GameManager.Instance().ROBO == null)
            return;
        for (int i = 0; i < m_EnergyBar.transform.childCount; i++)
        {
            Transform t = m_EnergyBar.transform.GetChild(i);

            if (i * 5.0f <= GameManager.Instance().ROBO.ENERGY)
            {
                t.gameObject.SetActive(true);
            }
            else
                t.gameObject.SetActive(false);
        }
    }

    public void EnergyUpdate(float curEnergy)
    {
        if (m_networkCheck)
            return;
        UpdateEnergy();
    }
}
