using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class PatternC : PatternState
{
    public PatternC(SkeletonAnimation ani , string moveAni , string attackAni,string aiTarget) : base(ani , moveAni , attackAni,aiTarget) { }
    private float m_tick = 0;


    // -- 상태 지정 ---------------------------------------------------------//

    private enum PATTERN_C
    {
        TRANSFORM = 0,
        DEF_MODE
    }
    private GameObject m_me = null;
    // ---------------------------------------------------------------------//

    public override void PatternStart()
    {
        m_skletonAnimation.state.SetAnimation(2 , "transform" , false);
        m_skletonAnimation.state.AddAnimation(2 , "attack_C_charge",false,0.0f);
        m_skletonAnimation.state.AddAnimation(2 , "attack_C_fire" , false , 0.0f);


        NetworkManager.Instance().SendOrderMessage(
            JSONMessageTool.ToJsonAIMessage(m_aiTarget,"C",new string[]{ "transform" , "attack_C_charge" , "attack_C_fire" }));

        m_skletonAnimation.gameObject.GetComponent<AudioSource>().Play();
        m_skletonAnimation.state.Complete += CompleteEvent;
        base.PatternStart();
    }

    public override float Attack(GameObject hero,GameObject me, int index)
    {
        m_me = me;
        m_tick += Time.deltaTime;

       
        if (m_tick >= GameSetting.BOSS1_PATTERN_C_ATTACK_COOLTIME)
        {
            m_tick = 0.0f;
        }
        return GameSetting.BOSS1_PATTERN_C_ATTACK_COOL;
    }

    void CompleteEvent(TrackEntry trackEntry)
    {
        if (trackEntry.animation.name == "attack_C_charge")
        {
            Stage1BOSS boss = m_me.GetComponent<Stage1BOSS>();
            boss.m_laser.gameObject.SetActive(true);
            boss.m_laser.Play("boss_laser");
            boss.m_laser.GetComponent<BoxCollider2D>().enabled = true;
            //   m_skeletonAnimation.state.ClearTrack(1);
        }
    }

    public override void Exit()
    {
        Stage1BOSS boss = m_me.GetComponent<Stage1BOSS>();
        boss.m_laser.SetInteger("laser" , 1);
        boss.m_laser.Play("Wait");
        boss.m_laser.gameObject.SetActive(false);
        boss.m_laser.GetComponent<BoxCollider2D>().enabled = false;
        m_skletonAnimation.state.ClearTrack(2);

        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonAIExitMessage(m_aiTarget,"C"));
    }

    public override float PreProcessedDamge()
    {
        // 데미지의 연산을 어떻게 처리할 것인가에 대한 것.
        // 패턴 A 에선 들어오는 데미지의 100%를 받는다.

        return 1.0f;
    }

    public override void Move(GameObject target, GameObject hero)
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
