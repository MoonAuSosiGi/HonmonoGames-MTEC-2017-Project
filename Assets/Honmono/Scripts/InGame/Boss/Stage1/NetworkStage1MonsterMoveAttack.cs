using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class NetworkStage1MonsterMoveAttack : MonoBehaviour, NetworkManager.NetworkMessageEventListenrer {

    // -- 기본정보 -------------------------------------------------------------------//
    private string m_name = null;
    private SkeletonAnimation m_skeletonAnimation = null;
    private MeshRenderer m_meshRenderer = null;

    public string NAME { get { return m_name; } set { m_name = value; } }
    
    // ------------------------------------------------------------------------------//


    void Start()
    {
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_meshRenderer = this.GetComponent<MeshRenderer>();
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
        try
        {
            m_skeletonAnimation.state.SetAnimation(0 , "idle" , true);
        }
        catch (Exception)
        {
            m_skeletonAnimation.state.SetAnimation(0 , "move" , true);
        }
    }

    void DamageEffect()
    {
        Color color = m_meshRenderer.material.color;

        color.g -= 0.5f;
        color.b -= 0.5f;
        m_meshRenderer.material.color = color;
        if (color.g <= 0.0f)
        {
            CancelInvoke("DamageEffect");
            InvokeRepeating("DamageEffectEnd" , 0.1f , 0.1f);
        }
    }
    void DamageEffectEnd()
    {
        Color color = m_meshRenderer.material.color;

        color.g += 0.3f;
        color.b += 0.3f;
        m_meshRenderer.material.color = color;
        if (color.g >= 1.0f)
        {
            CancelInvoke("DamageEffectEnd");
        }
    }


    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        switch (e.msgType)
        {
            case NetworkManager.REMOVE:
                if (e.targetName.Equals(m_name))
                {
                    NetworkManager.Instance().RemoveNetworkOrderMessageEventListener(this);
                    NetworkManager.Instance().RemoveNetworkEnemyMoveEventListener(this.GetComponent<NetworkMoving>());

                    GameObject.Destroy(gameObject);
                }
                break;
            case NetworkManager.STATE_CHANGE:
                if (e.targetName.Equals(m_name))
                {
                    if (IsInvoking("DamageEffect") || IsInvoking("DamageEffectEnd"))
                    {
                        CancelInvoke();
                        m_meshRenderer.material.color = Color.white;
                    }

                    InvokeRepeating("DamageEffect" , 0.1f , 0.1f);
                }
                break;
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
