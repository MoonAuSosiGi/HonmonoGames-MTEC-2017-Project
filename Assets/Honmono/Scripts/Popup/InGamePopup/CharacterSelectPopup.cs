﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class CharacterSelectPopup : MonoBehaviour {

    //-- 기본 정보 -------------------------------------------------------------------------//
    [SerializeField]
    private GameObject m_Left = null;

    [SerializeField]
    private GameObject m_Center = null;

    [SerializeField]
    private GameObject m_Right = null;

    [SerializeField]
    private List<Image> m_speedList = new List<Image>();

    [SerializeField]
    private List<Image> m_powerList = new List<Image>();

    [SerializeField]
    private List<Image> m_repairList = new List<Image>();

    // Tween 전용 --
    private Vector3 m_leftStart = Vector3.zero;
    private Vector3 m_centerStart = Vector3.zero;
    private Vector3 m_rightStart = Vector3.zero;

    private SkeletonGraphic m_sprLeft = null;
    private SkeletonGraphic m_sprCenter = null;
    private SkeletonGraphic m_sprRight = null;

    public AudioClip m_swipe = null;
    public AudioClip m_selectFinish = null;

    //-------------------------------------------------------------------------------------//

    public void CharacterSelectButton()
    {
        SoundManager.Instance().PlaySound(m_selectFinish);
        PopupManager.Instance().AddPopup("LobbyPopup");
        PopupManager.Instance().ClosePopup(gameObject);
    }

    //-------------------------------------------------------------------------------------//
    // Use this for initialization

    void Awake()
    {
        m_leftStart = m_Left.transform.position;
        m_centerStart = m_Center.transform.position;
        m_rightStart = m_Right.transform.position;
    }
    void Start () {
        m_leftStart = m_Left.transform.position;
        m_centerStart = m_Center.transform.position;
        m_rightStart = m_Right.transform.position;
        m_sprLeft = m_Left.GetComponent<SkeletonGraphic>();
        m_sprCenter = m_Center.GetComponent<SkeletonGraphic>();
        m_sprRight = m_Right.GetComponent<SkeletonGraphic>();

        m_Center.transform.SetAsFirstSibling();
        m_Left.transform.SetSiblingIndex(1);
        m_Right.transform.SetSiblingIndex(2);

        UIUpdate();

        SoundManager.Instance().PlayBGM(1);
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.LeftArrow) && (iTween.Count(m_Left) == 0 && iTween.Count(m_Center)==0 && iTween.Count(m_Right)==0))
            LeftMove();
        else if (Input.GetKeyUp(KeyCode.RightArrow) && (iTween.Count(m_Left) == 0 && iTween.Count(m_Center) == 0 && iTween.Count(m_Right) == 0))
            RightMove();


        if(iTween.Count(m_Left) == 0 && iTween.Count(m_Center) == 0 && iTween.Count(m_Right) == 0)
        {
            m_sprLeft = m_Left.GetComponent<SkeletonGraphic>();
            m_sprCenter = m_Center.GetComponent<SkeletonGraphic>();
            m_sprRight = m_Right.GetComponent<SkeletonGraphic>();
        }


    }


    //-- 캐릭터 이동 ---------------------------------------------------------------------//

    private void LeftButton()
    {
        SoundManager.Instance().PlaySound(m_swipe);
        LeftMove();
    }

    private void RightButton()
    {
        SoundManager.Instance().PlaySound(m_swipe);
        RightMove();
    }

    private void LeftMove()
    {
        m_Right.transform.SetSiblingIndex(2);
        m_Left.transform.SetSiblingIndex(1);
        Tween(m_Left, m_Right.transform.position, "left_r");
        Tween(m_Center, m_Left.transform.position, "center_l");
        Tween(m_Right, m_Center.transform.position, "right_c");
    }

    private void RightMove()
    {
        m_Right.transform.SetSiblingIndex(1);
        m_Left.transform.SetSiblingIndex(2);
        Tween(m_Left, m_Center.transform.position, "left_c");
        Tween(m_Center, m_Right.transform.position, "center_r");
        Tween(m_Right, m_Left.transform.position, "right_l");
    }

    //-----------------------------------------------------------------------------------//

    private void Tween(GameObject obj,Vector3 target,string dir)
    {
        iTween.MoveTo(obj, iTween.Hash("x", target.x,"y",target.y,
            "easetype","easeOutQuart", 
            "oncompletetarget", gameObject,
            "oncomplete", "TweenEnd", 
            "oncompleteparams",dir));
    }

    private void TweenEnd(string info)
    {
        string[] infos = info.Split('_');

        m_Center.transform.position = m_centerStart;
        m_Left.transform.position = m_leftStart;
        m_Right.transform.position = m_rightStart;

        MDebug.Log("t " + infos);
        if(infos[0] == "left")
        {
            if(infos[1] == "r")
            {
                // TODO 추후 캐릭터 추가시 여기서 바꿈 CENTER 로 바꿔야함
            }
            else
            {
                // TODO 추후 캐릭터 추가시 여기서 바꿈 RIGHT 로 바꿔야함
            }
        }
        else if (infos[0] == "center")
        {
            if (infos[1] == "r")
            {
                // TODO 추후 캐릭터 추가시 여기서 바꿈 left 로 바꿔야함
            }
            else
            {
                // TODO 추후 캐릭터 추가시 여기서 바꿈 right 로 바꿔야함
            }
        }
        else if (infos[0] == "right")
        {
            if (infos[1] == "l")
            {
                // TODO 추후 캐릭터 추가시 여기서 바꿈 CENTER 로 바꿔야함
            }
            else
            {
                // TODO 추후 캐릭터 추가시 여기서 바꿈 left 로 바꿔야함
            }
        }
    }

    //-- 능력치 조절 -------------------------------------------------------------------//

    public void SpeedControl(string objName)
    { 
        if (objName == "plus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_SPEED + 1 <= GameSetting.STAT.MAX_SPEED)
                GameManager.Instance().PLAYER.STATUS.STAT_SPEED++;
        }
        else if(objName == "minus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_SPEED - 1 >= 0)
                GameManager.Instance().PLAYER.STATUS.STAT_SPEED--;
        }
        UIUpdate();
    }

    public void PowerControl(string objName)
    {
        if (objName == "plus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_POWER + 1 <= GameSetting.STAT.MAX_POWER)
                GameManager.Instance().PLAYER.STATUS.STAT_POWER++;
        }
        else if (objName == "minus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_POWER - 1 >= 0)
                GameManager.Instance().PLAYER.STATUS.STAT_POWER--;
        }
        UIUpdate();
    }

    public void RefairControl(string objName)
    {
        if (objName == "plus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_REPAIR + 1 <= GameSetting.STAT.MAX_REPAIR)
                GameManager.Instance().PLAYER.STATUS.STAT_REPAIR++;
        }
        else if (objName == "minus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_REPAIR - 1 >= 0)
                GameManager.Instance().PLAYER.STATUS.STAT_REPAIR--;
        }
        
        UIUpdate();
    }

    //-- UI 갱신 -----------------------------------------------------------------------//

    private void UIUpdate()
    {
            //speed
        
        for(int i = 0; i < m_speedList.Count; i++)
        {
            if (i < GameManager.Instance().PLAYER.STATUS.STAT_SPEED)
                m_speedList[i].enabled = true;
            else
                m_speedList[i].enabled = false;
        }

        for (int i = 0; i < m_powerList.Count; i++)
        {
            if (i < GameManager.Instance().PLAYER.STATUS.STAT_POWER)
                m_powerList[i].enabled = true;
            else
                m_powerList[i].enabled = false;
        }

        for (int i = 0; i < m_repairList.Count; i++)
        {
            if (i < GameManager.Instance().PLAYER.STATUS.STAT_REPAIR)
                m_repairList[i].enabled = true;
            else
                m_repairList[i].enabled = false;
        }
    }
    
}