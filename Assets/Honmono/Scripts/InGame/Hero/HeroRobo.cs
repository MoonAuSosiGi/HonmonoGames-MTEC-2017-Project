﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HeroRobo : MonoBehaviour {

    public enum ROBO_STATE
    {
       NONE = 0,
       WEAPON_FIRE      = 4,  
       WEAPON_RELOAD    = 5   
    }

    //------------------------------------------------//
    //Character 기본 정보

    // 주인공인지 다른사람인지
    [SerializeField]
    private bool m_isMe = true;

    // 추진체 발광
    [SerializeField]
    private Light m_fireLight = null; 

    // 이동을 제외한 상태가 지정된다.
    private ROBO_STATE  m_roboState = ROBO_STATE.NONE;

    // Reload 속도 -- 
    private float m_reloadSpeed = 0.3f;

    // Move 속도 
    private float m_moveSpeed = 10.0f;


    //-- normal map animation -----------------------//
    // normal map 의 경우 unity animation 에서
    // 변경이 불가능한 것으로 보이므로 스크립트 내에서 바꿈
    // [일단은 임시 구현] TODO 더 나은 방법을 찾기
    [SerializeField]
    private List<Texture> m_flyNormalList = new List<Texture>();
    [SerializeField]
    private List<Texture> m_fireNormalList = new List<Texture>();

    private List<Texture> m_currentNormalList = null;

    private int m_currentNormalIndex = 0;
    

    //-- animation --//
    private void ChangeNormalAnimation()
    {
        if (m_renderer == null || m_currentNormalList == null)
            return;
        this.m_renderer.material.SetTexture("_BumpMap", m_currentNormalList[m_currentNormalIndex++]);

        if (m_currentNormalIndex >= m_currentNormalList.Count)
            m_currentNormalIndex = 0;
    }
    private void SetupNormalAnimation(int type)
    {
        // type  = 0  fly  type = 1 fire
        this.m_currentNormalList = (type == 0) ? this.m_flyNormalList : this.m_fireNormalList;
        m_currentNormalIndex = 0;
        this.m_renderer.material.SetTexture("_BumpMap", m_currentNormalList[m_currentNormalIndex++]);
    }

    //-----------------------------------------------//



    private SpriteRenderer m_renderer = null;
    private Animator m_animator = null;

    //-----------------------------------------------//
	// Use this for initialization
	void Start () {

        m_renderer = this.GetComponent<SpriteRenderer>();
        m_animator = this.GetComponent<Animator>();

        LightRangeUp();

    }

    void LightUpdate(float s)
    {
        m_fireLight.range = s;
    }

    void LightRangeUp()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 1.0f, "to", 1.7f, "time", 0.3f, "onupdatetarget", gameObject, "onupdate", "LightUpdate", "oncompletetarget", gameObject, "oncomplete", "LightRangeDown"));
    }


    void LightRangeDown()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 1.7f, "to", 1.0f, "time", 0.3f, "onupdatetarget", gameObject, "onupdate", "LightUpdate", "oncompletetarget", gameObject, "oncomplete", "LightRangeUp"));
    }

	
	// Update is called once per frame
	void Update () {

        if (m_isMe)
            Control();
        else
            NetworkUpdate();
        
	}

    /// <summary>
    /// 캐릭터 조작에 관련된 함수
    /// </summary>
    void Control()
    {
        Vector3 pos = transform.position;
        Vector3 lightPos = m_fireLight.transform.localPosition;

        float movex = 0;
        float movey = 0;
        

        // Horizontal
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movex = -m_moveSpeed * Time.deltaTime;
            this.GetComponent<SpriteRenderer>().flipX = false;
            //불빛 위치 변경
            this.m_fireLight.transform.localPosition = new Vector3(0.7f, lightPos.y, lightPos.z);
            
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movex = m_moveSpeed * Time.deltaTime;
            this.GetComponent<SpriteRenderer>().flipX = true;
            //불빛 위치 변경
            this.m_fireLight.transform.localPosition = new Vector3(-0.7f, lightPos.y, lightPos.z);
        }
        // Vertical
        if (Input.GetKey(KeyCode.UpArrow))
            movey = m_moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow))
            movey = -m_moveSpeed * Time.deltaTime;
        
        if (Input.GetKey(KeyCode.Space))
        {
            // None 으로 해도 무방하지만 혹시 모를 상태 추가에 대비
            if(this.m_roboState != ROBO_STATE.WEAPON_FIRE || this.m_roboState != ROBO_STATE.WEAPON_RELOAD)
            {
                this.m_roboState = ROBO_STATE.WEAPON_FIRE;
                this.m_animator.SetInteger("STATE", (int)m_roboState);
            }
            
        }

        // TODO 추후 MapManager 에서 가져오는 형태로 바꿀것
        TargetMoveCamera camera = Camera.main.GetComponent<TargetMoveCamera>();
        Vector3 backPos = camera.BACKGROUND_POS;
        float leftCheck = backPos.x - camera.BACKGROUND_HALF_WIDTH;
        float rightCheck = backPos.x + camera.BACKGROUND_HALF_WIDTH;
        float upCheck = backPos.y + camera.BACKGROUND_HALF_HEIGHT;
        float downCheck = backPos.x - camera.BACKGROUND_HALF_HEIGHT;

        float w = (m_renderer.flipX) ? m_renderer.bounds.size.x / 2.0f : -m_renderer.bounds.size.x / 2.0f;
        float h = (movey >= 0) ? -m_renderer.bounds.size.y / 2.0f : m_renderer.bounds.size.y / 2.0f;
        if (pos.x + movex + w <= leftCheck)     movex = 0.0f;
        if (pos.x + movex + w >= rightCheck)    movex = 0.0f;
        if (pos.y + movey >= upCheck + h)       movey = 0.0f;
        if (pos.y + movey <= downCheck + h)     movey = 0.0f;
        
        transform.Translate(movex, movey, 0);
    }


    /// <summary>
    /// 네트워크 업데이트 ( 실시간으로 무언가를 받아와야 한다 )
    /// </summary>
    void NetworkUpdate()
    {
        
    }

    void FireBullet()
    {
        this.m_roboState = ROBO_STATE.WEAPON_RELOAD;
        GameObject bullet =  BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_HERO_DEF,m_renderer.flipX);
        Vector3 pos = transform.position;
        float width = (m_renderer.flipX) ? m_renderer.bounds.size.x / 2.0f : -m_renderer.bounds.size.x / 2.0f;
        bullet.transform.position = new Vector3(pos.x + width, pos.y, pos.z);

        Invoke("WeaponReload", m_reloadSpeed);
    }

    void FireEnd()
    {
        
    }

    void WeaponReload()
    {
        this.m_roboState = ROBO_STATE.NONE;
        this.m_animator.SetInteger("STATE", (int)m_roboState);
        this.m_animator.Play("Robo_fly");
    }

    void OnGUI()
    {
        GUI.TextArea(new Rect(0, 20, 250, 30), "RELOAD SPEED : "+this.m_reloadSpeed);
        this.m_reloadSpeed = GUI.HorizontalSlider(new Rect(0, 50, 100, 30), this.m_reloadSpeed, 0.0f, 1.0f);
    }

}