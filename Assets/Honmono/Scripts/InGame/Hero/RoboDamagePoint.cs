using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboDamagePoint : MonoBehaviour,NetworkManager.NetworkMessageEventListenrer {

    // -- 기본정보 ---------------------------------------------------------------//
    SpriteRenderer m_renderer = null;
    int m_index = 3;

    string m_name = null;

    bool m_isNetworkObject = false;

    public List<Sprite> m_sprList = new List<Sprite>();

    // -------------------------------------------------------------------------- //

    public string NETWORK_NAME
    {
        get { return m_name; }
        set { m_name = value; }
    }

    public bool NETWORK_OBJECT
    {
        get { return m_isNetworkObject; }
        set {
            m_isNetworkObject = value;
            if (m_isNetworkObject)
            {
                this.GetComponent<BoxCollider2D>().enabled = false;
                NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
            }
        }
    }

    void Start()
    {
        m_renderer = this.GetComponent<SpriteRenderer>();
    }

    public void DamageFix()
    {
        this.GetComponent<Animator>().enabled = false;
        if (m_index <= 0)
        {
            // hp up
            // Destroy
            if (m_isNetworkObject)
                NetworkManager.Instance().RemoveNetworkOrderMessageEventListener(this);
            else
                GameManager.Instance().ROBO.Heal(10);
            
            MapManager.Instance().RemoveObject(gameObject);
            return;
        }

        m_index--;
        m_renderer.sprite = m_sprList[m_index];

        if (!m_isNetworkObject)
            StateSend();
        
    }

    void StateSend()
    {
        NetworkManager.Instance().SendOrderMessage(
            JSONMessageTool.ToJsonOrderStateValueChange(m_name , m_index));
    }

    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if(e.targetName.Equals(m_name))
        {
            m_index = (int)e.msg.GetField(NetworkManager.STATE_CHANGE).i;
            DamageFix();
        }
    }
}
