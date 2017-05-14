using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public abstract class PatternState  {

    protected SkeletonAnimation m_skletonAnimation = null;
    protected string m_moveAni = null;
    protected string m_attackAni = null;
    protected string m_aiTarget = null;

    public PatternState(SkeletonAnimation ani,string moveAni, string attackAni,string aiTarget)
    {
        m_skletonAnimation = ani;
        m_moveAni = moveAni;
        m_attackAni = attackAni;
        m_aiTarget = aiTarget;
        PatternStart();
    }

    // Attack 의 경우 여기서 따로 처리해도 무방
    // 따로 쿨타임을 처리해야할 필요성이 있을 때를 대비해서 float 를 리턴한다.
    public abstract float Attack(GameObject hero,GameObject me,int index);

    // 패턴 시작시 호출
    public virtual void PatternStart() { }

    // 다만 데미지의 경우 실 계산은 해당 오브젝트에서 해야된다.
    // 데미지를 얼만큼 받을지에 대한 처리로 float 형으로 리턴하는 값을 퍼센트 적용해서 처리한다.
    public abstract float PreProcessedDamge();

    // 실제 이동 로직을 처리한다.
    // 이녀석은 Update 에서 호출되어야 한다.
    public abstract void Move(GameObject target,GameObject hero);

    // 내부 업데이트문
    public virtual void Update(GameObject me) { }
    // 패턴 종료시
    public virtual void Exit() { }
    
    // -- 애니메이션 플레이 ---------------------------------------------------------------------//
    protected void MoveAniPlay()
    {
        if(m_skletonAnimation != null && !string.IsNullOrEmpty(m_moveAni))
            m_skletonAnimation.state.SetAnimation(0 , m_moveAni , true);
    }   

    protected void AttackAniPlay()
    {
        if (m_skletonAnimation != null && !string.IsNullOrEmpty(m_attackAni))
            m_skletonAnimation.state.SetAnimation(0 , m_attackAni , true);
    }
}
