using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1BOSS : BOSS {


    // -- 기본 정보 --------------------------------------------------------------------------------//
    
    GameObject m_hero = null;


    // AI 체크 변수들

    // 공격 가능 범위 체크
    private float m_attackableTick = 0.0f;

    // Pattern A 조건 체크 Tick
    private float m_patternATick = 0.0f;

    // Pattern B 조건 체크 Tick
    private float m_patternBTick = 0.0f;
    
    // Pattern C 조건 체크 Tick
    private float m_patternCTick = 0.0f;


    // 쿨타임을 실제 계산할 변수
    private float m_coolTimeTick = 0.0f;

    // 몇초동안 쉬는지
    private float m_coolTime = 0.0f;
    // --------------------------------------------------------------------------------------------//

    // AI 를 보고싶다
    void OnGUI()
    {
        if (m_pattern == null)
            return;
        string txt = "";
        float tick = 0.0f;
        float coolTime = m_coolTimeTick;

        txt = m_pattern.GetType().ToString();
        if (m_pattern is PatternA)
            tick = m_patternATick;
        else if (m_pattern is PatternB)
            tick = m_patternBTick;
        else if (m_pattern is PatternC)
            tick = m_patternCTick;
        else if (m_pattern is PatternNormal)
            tick = m_attackableTick;

       
        GUI.TextArea(new Rect(500, 150, 100, 100), txt + " tick : " + tick + " coolTime ? : " + coolTime);
    }

    void Start()
    {
        
        // 처음 패턴은 A다.
        m_pattern = new PatternA();
       
    }


    void Update()
    {

        if (NetworkOrderController.ORDER_NAME != GameManager.Instance().PLAYER.USER_NAME
            && NetworkOrderController.ORDER_SPACE != 0)
            return;
        

        // 체력이 30% 이하로 떨어지면 
        if(m_hp <= m_fullHp * GameSetting.BOSS1_PATTERN_D_HP_CONDITION)
        {
            // 광폭화 모드가 아니라면 광폭화 모드다 !!!!
            if(!(m_pattern is PatternD))
            {
                m_pattern = new PatternD();
            }
            
        }
        else
        {
            if(!AttackAbleCheck())
            {
                if(!PatternA_AbleCheck())
                {
                    if(!PatternB_AbleCheck())
                    {
                        if (!PatternC_AbleCheck())
                        {
                            m_attackableTick = 0.0f;
                            m_patternATick = 0.0f;
                            m_patternBTick = 0.0f;
                            m_patternCTick = 0.0f;
                            
                        }
                    }
                }
                
            }
        }
        // 쿨타임 중이면 아~~무것도 안함
        if (CoolTime())
            return;

        SetCoolTime(Attack());
        Move();

    }

    void SetCoolTime(float time)
    {
        if (time <= 0.0f)
            return;
        m_coolTime = time;
        m_coolTimeTick = 0.0f;
    }

    bool CoolTime()
    {
        
        if(m_coolTimeTick > m_coolTime)
        {
            // m_coolTime = 0.0f;
       //     m_coolTimeTick = m_coolTime;
            //  MDebug.Log("쿨타임 아님 "+m_coolTimeTick + " " + m_coolTime);
            return false;
        }
        else
        {
            
          
            m_coolTimeTick += Time.deltaTime;
            return true;
        }
        
    }

    // 공격 가능 범위 체크용
    bool AttackAbleCheck()
    {
        if(m_hero == null)
            m_hero = GameManager.Instance().PLAYER.PLAYER_HERO.gameObject;
        m_attackableTick += Time.deltaTime;

        if(m_attackableTick >= GameSetting.BOSS1_ATTACK_ABLE_COOLTIME)
        {

            return false;
        }
        // 3 - 4초간 체크한다.
        Vector3 heroPos = m_hero.transform.position;
        Vector3 pos = transform.position;

        // 공격가능 범위인가?
        if (Vector3.Distance(heroPos, pos) <= GameSetting.BOSS1_ATTACK_ABLE_DISTANCE)
        {
            //기본 공격!!

            if (!(m_pattern is PatternNormal))
                m_pattern = new PatternNormal();

        }

        return true;
        
    }

    // 패턴 A 체크용
    bool PatternA_AbleCheck()
    {
        m_patternATick += Time.deltaTime;

        if(m_patternATick >= GameSetting.BOSS1_PATTERN_A_ABLE_COOLTIME)
        {
            return false;
        }

        // 이 시간 동안에는 패턴 A로 공격한다
        if (!(m_pattern is PatternA))
            m_pattern = new PatternA();

        return true;
    }

    // 패턴 B 체크용
    bool PatternB_AbleCheck()
    {
        m_patternBTick += Time.deltaTime;

        if (m_patternBTick >= GameSetting.BOSS1_PATTERN_B_ABLE_COOLTIME)
        {
            return false;
        }

        // 이 시간 동안에는 패턴 B로 공격한다
        if (!(m_pattern is PatternB))
            m_pattern = new PatternB();

        return true;
    }

    // 패턴 C 체크용
    bool PatternC_AbleCheck()
    {
        m_patternCTick += Time.deltaTime;

        if (m_patternCTick >= GameSetting.BOSS1_PATTERN_C_ABLE_COOLTIME)
        {
            return false;
        }

        // 이 시간 동안에는 패턴 C로 공격한다
        if (!(m_pattern is PatternC))
            m_pattern = new PatternC();

        return true;
    }
}
