using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class PatternA : PatternState {

    private float m_tick = 0.0f;
    //자기자신
    private GameObject m_monster = null;
    private GameObject m_hero = null;

    public PatternA(SkeletonAnimation ani,string moveAni,string attackAni, string aiTarget) : base(ani,moveAni,attackAni,aiTarget)
    {
        m_skletonAnimation.state.Complete += State_Complete;
        m_skletonAnimation.state.SetAnimation(0 , m_moveAni , true);
        NetworkManager.Instance().SendOrderMessage(
               JSONMessageTool.ToJsonAIMessage(
                   m_aiTarget,
                   "A" ,
                   m_moveAni ,
                   true));
    }

    private void State_Complete(Spine.TrackEntry trackEntry)
    {
        if(trackEntry.animation.name.Equals(m_attackAni))
        {
            if (m_tick >= GameSetting.BOSS1_PATTERN_A_ATTACK_COOLTIME)
            {
                Bullet b = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_BOSS1_P1);
                Vector3 pos = m_monster.transform.position;
                
                b.transform.position = pos;

                Vector3 dir = m_hero.transform.position - m_monster.transform.position;
                dir.Normalize();
                string name = GameManager.Instance().PLAYER.USER_NAME + "_boss_A_" + Monster.m_index++;

                b.BULLET_SPEED = 20.0f;
                b.transform.position = m_monster.transform.position;
                b.SetupBullet(name , false , dir);

                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonCreateOrder(
                        name , 
                        "boss1_bullet" ,
                        pos.x , pos.y ,
                        b.transform.rotation.eulerAngles.y , 
                       false));
                // 총알은 2.5초에 걸쳐 플레이어에게 도착함
                // 총알의 계산은 총알에서 하도록 함
                m_tick = 0.0f;

                //m_monster.GetComponent<Stage1BOSS>().m_attackEffect.Play("boss_shotEffect");

                NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessage(
                   m_aiTarget ,
                   "A" ,
                   m_moveAni ,
                   true));
                m_skletonAnimation.state.SetAnimation(0 , m_moveAni , true);
            }
        }
    }

    public override float Attack(GameObject hero,GameObject me, int index)
    {
        m_tick += Time.deltaTime;
        m_hero = hero;
        m_monster = me;
        // 패턴 A
        // 플레이어 위치를 추적하여 직선으로 날아가는 탄환 발사

        // 탄환 3발 발사
        // 2초마다 한번씩
        if(m_tick >= GameSetting.BOSS1_PATTERN_A_ATTACK_COOLTIME)
        {
            m_skletonAnimation.state.SetAnimation(0 , m_attackAni , false);
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessage(
                    m_aiTarget,
                    "A",
                    m_attackAni ,
                    false));
            return GameSetting.BOSS1_PATTERN_A_ATTACK_COOL;
        }

        return 0.0f;        
    }

    public override float PreProcessedDamge()
    {
        // 데미지의 연산을 어떻게 처리할 것인가에 대한 것.
        // 패턴 A 에선 들어오는 데미지의 100%를 받는다.

        return 1.0f;
    }

    public override void Move(GameObject target,GameObject hero)
    {
        float distance = Vector3.Distance(target.transform.position , 
            GameManager.Instance().ROBO.transform.position);

        if (distance >= 15.0f)
        {
            Vector3 dir = hero.transform.position - target.transform.position;
            dir.Normalize();

            // 임의로 랜덤이동
            target.transform.position += dir * GameSetting.BOSS1_SPEED * Time.deltaTime;
        }
    }


}
