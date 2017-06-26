using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

public class NetworkStage1MonsterStopAttack : MonoBehaviour, NetworkManager.NetworkMessageEventListenrer
{


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
        m_skeletonAnimation.state.SetAnimation(0 , "idle" , true);
        m_skeletonAnimation.state.Complete += State_Complete;
    }

    private void State_Complete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.animation.name.Equals("attack"))
        {
            m_skeletonAnimation.state.SetAnimation(0 , "idle" , true);
            //  m_target = null;
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
            case NetworkManager.HP_UPDATE:
                if (e.targetName.Equals(m_name))
                {
                    GameManager.Instance().SetCurrentEnemy(GetComponent<Monster>());
                    if (IsInvoking("DamageEffect") || IsInvoking("DamageEffectEnd"))
                    {
                        CancelInvoke();
                        m_meshRenderer.material.color = Color.white;
                    }

                    InvokeRepeating("DamageEffect" , 0.1f , 0.1f);
                }
                break;
            case NetworkManager.STATE_CHANGE:
                
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

                AudioSource source = GetComponent<AudioSource>();
                if (source != null && !source.isPlaying)
                    source.Play();
                break;
        }
    }
}
