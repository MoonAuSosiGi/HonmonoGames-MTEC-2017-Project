using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Stage1Monster : Monster
{

    // -- 기본 정보 --------------------------------------------------------//
    private HeroRobo m_robo = null;

    // 공격 가능 범위 체크용
    private float m_attackableTick = 0.0f;

    // 쿨타임을 실제 계산할 변수
    private float m_coolTimeTick = 0.0f;

    // 몇초동안 쉬는지
    private float m_coolTime = 0.0f;

    private SkeletonAnimation m_skletonAnimation = null;
    private const string ANI_ATTACK = "attack";
    private const string ANI_IDLE = "idle";
    private const string ANI_MOVE = "move";

    // ---------------------------------------------------------------------//

    void Start()
    {
        m_robo = GameManager.Instance().ROBO;
        m_skletonAnimation = this.GetComponent<SkeletonAnimation>();
        this.m_skletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
        m_pattern = new MonsterPattern(m_skletonAnimation , null , null , null);
    }


    void Update()
    {
        if (FindMoveAbleCheck())
            Move();
        if (!AttackAbleCheck())
            return;
        // 쿨타임 중이면 아~~무것도 안함
        if (CoolTime())
            return;

        SetCoolTime(Attack());

    }

    public override float Attack()
    {
        this.m_skletonAnimation.state.SetAnimation(0 , ANI_ATTACK , false);
        this.m_skletonAnimation.state.AddAnimation(0 , ANI_IDLE , true , 0.0f);
        return base.Attack();
    }

    protected override void Move()
    {
        if (m_pattern != null)
            m_pattern.Move(gameObject , m_robo.gameObject);

        if (m_skletonAnimation.state.GetCurrent(0).animation.name == ANI_IDLE)
            this.m_skletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);

        if (m_robo.transform.position.x < transform.position.x)
            m_skletonAnimation.skeleton.flipX = false;
        else
            m_skletonAnimation.skeleton.flipX = true;
        base.Move();
    }

    public override void Damage(float damage)
    {
        base.Damage(damage);

        if (m_hp <= 0.0f)
        {
            GameObject obj = MapManager.Instance().AddObject(GamePath.EFFECT);
            obj.transform.position = transform.position;
            GameObject.Destroy(gameObject);
        }
    }

    // 공격 가능 범위 체크
    bool AttackAbleCheck()
    {
        Vector3 roboPos = m_robo.transform.position;
        Vector3 pos = transform.position;

        if (Vector3.Distance(roboPos , pos) <= GameSetting.MONSTER_ATTACK_DISTANCE)
        {
            return true;
        }

        return false;
    }

    bool FindMoveAbleCheck()
    {
        Vector3 roboPos = m_robo.transform.position;
        Vector3 pos = transform.position;

        if (Vector3.Distance(roboPos , pos) <= GameSetting.MONSTER_FIND_DISTANCE)
        {
            return true;
        }

        return false;
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

        if (m_coolTimeTick > m_coolTime)
        {
            return false;
        }
        else
        {


            m_coolTimeTick += Time.deltaTime;
            return true;
        } 

    }
    //-------------------------------------------------------------------------------//

    void OnTriggerEnter2D(Collider2D col)
    {
        switch(col.tag)
        {
            case "Player":
                HeroRobo robo = col.GetComponent<HeroRobo>();

                if(robo != null)
                {
                    // 로봇이 아닌 경우다 
                    Hero hero = col.GetComponent<Hero>();
                }
                else
                {
                    //로봇인 경우 
                    robo.Damage(1.0f); // 임시값 
                }
                
                break;

        }
    }
}
