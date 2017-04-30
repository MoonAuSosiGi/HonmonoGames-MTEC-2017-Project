using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class PatternD : PatternState
{
    public PatternD(SkeletonAnimation ani) : base(ani) { }

    private float m_tick = 0;
    private bool m_attack = false;
    private GameObject m_me = null;
    private GameObject m_rotate = null;

    private bool m_effect = false;
    private bool m_moveEffect = false;

    private float m_angle = 0.0f;
    public override bool GetAttack()
    {
        return m_attack;
    }

    public override void PatternStart()
    {
        m_skletonAnimation.state.SetAnimation(2 , "transform" , false);
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonAIMessageAnimation("D" , "transform" , 2 , false));
        m_skletonAnimation.state.Complete += State_Complete;
    }

    private void State_Complete(Spine.TrackEntry trackEntry)
    {
        if(trackEntry.animation.name == "transform")
        {
            m_rotate = m_me.GetComponent<Stage1BOSS>().m_patternDRotate;
            m_rotate.transform.position = m_me.transform.position;
            m_rotate.gameObject.SetActive(true);
            m_rotate.transform.localScale = new Vector3(3 ,3);

            // 다시 생겼을 때 
            if (m_moveEffect)
            {
                m_effect = false;
                iTween.ScaleTo(m_rotate , iTween.Hash("x" , 0.0f , "y" , 0.0f));
                m_skletonAnimation.state.SetAnimation(2 , "move_fast_close" , true);

                if (!m_me.GetComponent<Stage1BOSS>().m_networkObject)
                {
                    NetworkManager.Instance().SendOrderMessage(
                        JSONMessageTool.ToJsonAIMessageAnimation("D" , "move_fast_close" , 2 , true));
                    NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonAIMessageD_END());
                }
            }
            else
            {
                m_effect = true;
                NetworkManager.Instance().SendOrderMessage(
                        JSONMessageTool.ToJsonAIMessageAnimation("D" , "attack_D_in" , 2 , true));
                m_skletonAnimation.state.SetAnimation(2 , "attack_D_in" , false);
            }
            
            
            
        }
        if(trackEntry.animation.name == "attack_D_in")
        {
            
        }
    }

    public override void Update(GameObject me)
    {
        m_me = me;

        if(m_effect)
        {
            m_rotate.transform.Rotate(0.0f , 0.0f , m_angle);
            m_angle += 0.1f;
            m_rotate.transform.position = me.transform.position;
            m_tick += Time.deltaTime;

            if (!m_me.GetComponent<Stage1BOSS>().m_networkObject)
            {
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonAIMessageD_Rotate(m_angle,m_rotate.transform.position));
            }

            if (m_tick >= GameSetting.BOSS1_PATTERN_D_SPECIAL && !m_moveEffect)
            {

                me.transform.parent.position = GameManager.Instance().ROBO.transform.position;
                
                m_moveEffect = true;
                //m_tick = 0.0f;
                m_skletonAnimation.state.SetAnimation(2 , "transform" , false);
                m_skletonAnimation.state.Complete += State_Complete;

                if (!m_me.GetComponent<Stage1BOSS>().m_networkObject)
                {
                    NetworkManager.Instance().SendOrderMessage(
                        JSONMessageTool.ToJsonAIMessageAnimation("D" , "transform" , 2 , false));
                }
            }
            Vector3 p = me.transform.parent.position;
            //NetworkManager.Instance().SendEnemyMoveMessage(JSONMessageTool.ToJsoinEnemyMove(m_me.GetComponent<Stage1BOSS>().m_BOSS_NAME , p.x , p.y , p.z , false));
        }
    }

    public override float Attack(GameObject hero,GameObject me,int index)
    {
        // 패턴 D
        // 특수패턴

        // 일정 시간 지난 후
        if (m_moveEffect)
            m_tick += Time.deltaTime;

        if (m_moveEffect && m_tick >= GameSetting.BOSS1_PATTERN_D_SPECIAL)
        {

            MDebug.Log("데미지 데미지! ");
            m_tick = 0.0f;
        }

            return 0.0f;
    }

    public override float PreProcessedDamge()
    {
        // 데미지의 연산을 어떻게 처리할 것인가에 대한 것.
        // 데미지 경감

        return GameSetting.BOSS1_PATTERN_D_DEF;
    }

    public override void Move(GameObject target, GameObject hero)
    {
        Vector3 dir = hero.transform.position - target.transform.position;
        dir.Normalize();

        // 임의로 랜덤이동
        target.transform.position += dir * GameSetting.BOSS1_SPEED * Time.deltaTime;
    }
}
