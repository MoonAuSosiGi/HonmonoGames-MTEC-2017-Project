using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkStage2BOSS : MonoBehaviour, NetworkManager.NetworkMessageEventListenrer
{
    private List<SpriteRenderer> m_destroyBoneList = new List<SpriteRenderer>();
    private string m_name = null;

    public string BOSS_NAME { get { return m_name; } set { m_name = value; } }

    void DestroyListSetup()
    {
        Transform t = transform;
        while (m_destroyBoneList.Count < 10)
        {
            t = t.GetChild(0);

            if (t.childCount >= 2)
            {
                m_destroyBoneList.Add(t.GetChild(1).GetComponent<SpriteRenderer>());
            }
        }
    }


    void Start()
    {
        DestroyListSetup();
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
    }

    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if(e.msgType.Equals(NetworkManager.PART_DESTROY))
        {
            m_destroyBoneList[(int)e.msg.GetField(NetworkManager.PART_DESTROY).i].enabled = true;
        }
        else if (e.msgType.Equals(NetworkManager.HP_UPDATE))
        {
            if (e.targetName.Equals(m_name))
            {
                GameManager.Instance().SetCurrentEnemy(GetComponent<Monster>());

            }
        }

        
    }
}
