using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class NetworkMonster : MonoBehaviour, NetworkManager.NetworkMessageEventListenrer
{
    private string m_name = null;
    private SkeletonAnimation m_skeletonAnimation = null;
  //  private TextMesh m_text = null;

    public string NAME { get { return m_name; } set { m_name = value; } }

    void Start()
    {
      //  m_text = transform.GetChild(0).GetComponent<TextMesh>();
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
        try
        {
            m_skeletonAnimation.state.SetAnimation(0 , "idle" , true);
        }catch(Exception)
        {
            m_skeletonAnimation.state.SetAnimation(0 , "move" , true);
        }
        
    }

    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        switch (e.msgType)
        {
            case NetworkManager.REMOVE:
                if(e.targetName.Equals(m_name))
                {
                    NetworkManager.Instance().RemoveNetworkOrderMessageEventListener(this);
                    NetworkManager.Instance().RemoveNetworkEnemyMoveEventListener(this.GetComponent<NetworkMoving>());
                    
                    GameObject.Destroy(gameObject);
                }
                break;
            case NetworkManager.STATE_CHANGE:
                //if(e.targetName.Equals(m_name))
                //    m_text.text = "HP : " + e.msg.GetField(NetworkManager.STATE_CHANGE).i;
                //break;
            case NetworkManager.AI_ANI_NAME:
                if (!e.targetName.Equals(m_name) || m_skeletonAnimation == null ||
                    e.msg.GetField(NetworkManager.AI_ANI_NAME) == null ||
                    m_skeletonAnimation.state.GetCurrent(0) == null ||
                    m_skeletonAnimation.state.GetCurrent(0).animation == null ||
                    m_skeletonAnimation.state.GetCurrent(0).animation.name == null)
                    return;
                if (m_skeletonAnimation.state.GetCurrent(0).animation.name.Equals(e.msg.GetField(NetworkManager.AI_ANI_NAME).str))
                    return;
                
                m_skeletonAnimation.state.SetAnimation(0 ,
                            e.msg.GetField(NetworkManager.AI_ANI_NAME).str ,
                            e.msg.GetField(NetworkManager.AI_ANI_LOOP).b);

                break;
        }
    }
}
