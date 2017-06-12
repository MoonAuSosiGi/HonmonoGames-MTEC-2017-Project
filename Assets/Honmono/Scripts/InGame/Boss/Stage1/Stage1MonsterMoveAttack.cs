using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1MonsterMoveAttack : Monster {

    // -- 기본 정보 ------------------------------------------------//
    public GameObject m_target = null;

    private const string ANI_IDLE = "idle";
    private const string ANI_MOVE = "move";
    private const string ANI_ATTACK = "attack";
    // 쿨타임을 실제 계산할 변수
    private float m_coolTimeTick = 0.0f;

    // 몇초동안 쉬는지
    private float m_coolTime = 0.0f;

    MeshRenderer m_meshRenderer = null;
    private bool m_damageCoolTime = false;

    // Networking
    Vector3 m_prevPos = Vector3.zero;
    float m_lastSendTime = 0.0f;
    private string m_prevState = null;
    private string m_curState = null;

    private bool m_isNetworkObject = false;
    bool m_networkObjectCheck = false;
    public bool NETWORKING { set { m_isNetworkObject = value; } }
    // -------------------------------------------------------------//

    void Start()
    {
        //m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        //m_skeletonAnimation.state.Complete += State_Complete;
        m_meshRenderer = this.GetComponent<MeshRenderer>();
        m_curState = "idle";
        this.m_skeletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);

        m_hp = 5;
        m_fullHp = 5;
    }
}
