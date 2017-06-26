using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPopup : MonoBehaviour
{

    // -- 상호작용시 발생하는 팝업 --------------------------------------------- //
    public GameObject m_cwHexagon = null;
    public GameObject m_ccwHexagon = null;
    public GameObject m_scaleUpDownHexagon = null;
    public GameObject m_cwSlowHexagon = null;
    public GameObject m_randomLeftSide = null;
    public GameObject m_randomRightSide = null;
    public Text m_text = null;
    // ------------------------------------------------------------------------- //
    // tween 용
    class TweenObj
    {
        public bool upTweenEnd = false;
        public GameObject target = null;
        public float speed = 0.1f;
        public bool cw = false;
        public TweenObj(float speed , GameObject t,bool cw) { this.speed = speed;  target = t; this.cw = cw; }
        public TweenObj(bool b , GameObject t) { upTweenEnd = b; target = t; }
    }

    public void SetText(string text)
    {
        m_text.text = text;
    }

    void Start()
    {
        GameManager.Instance().UIShow(false);
        // TEST CODE :: 
        // side tween 
        
        int rand = Random.Range(0 , 100);

        if (rand % 3 == 0)
        {
            SideTweenUp(m_randomLeftSide);
            SideTweenDown(m_randomRightSide);
        }
        else
        {
            SideTweenDown(m_randomLeftSide);
            SideTweenUp(m_randomRightSide);
        }

        // TEST CODE ::
        // rotate Start
        RotateAnimation(m_cwHexagon , 8.0f , true);
        RotateAnimation(m_ccwHexagon , 8.0f , false);
        RotateAnimation(m_cwSlowHexagon , 4.0f , true);
        Invoke("TimeOver" , 1.0f);
        //ScaleUpAndDownAnimation(m_scaleUpDownHexagon , true);
    }

    //TEST CODE ::
    void TimeOver()
    {
        ScaleUpAnimation(m_cwHexagon);
        ScaleUpAnimation(m_ccwHexagon);
        ScaleUpAnimation(m_cwSlowHexagon,2.0f);
        ScaleUpAnimation(m_scaleUpDownHexagon);
        m_randomRightSide.SetActive(false);
        m_randomLeftSide.SetActive(false);
        m_text.gameObject.SetActive(false);
        //ScaleUpAnimation(m_randomLeftSide);
        //ScaleUpAnimation(m_randomRightSide);
    }

    // 양쪽 사이드에서 위 아래로 움직이는 오브젝트 ----------------------------- //
    void SideTweenUp(GameObject obj)
    {
        iTween.MoveBy(obj , iTween.Hash(
            "y" , 100.0f ,
            "time",0.35f,
            "easetype" , "easeOutElastic" ,
            "oncompletetarget" , gameObject ,
            "oncomplete" , "SideTweenEnd" ,
            "oncompleteparams" , new TweenObj(true , obj)));
    }

    void SideTweenEnd(TweenObj tweenObj)
    {
        if (tweenObj.upTweenEnd)
        {
            SideTweenDown(tweenObj.target);
        }
        else
            SideTweenUp(tweenObj.target);
    }

    void SideTweenDown(GameObject obj)
    {
        iTween.MoveBy(obj , iTween.Hash(
            "y" , -100.0f ,
            "time" , 0.35f ,
            "easetype" , "easeOutElastic" ,
            "oncompletetarget" , gameObject ,
            "oncomplete" , "SideTweenEnd" ,
            "oncompleteparams" , new TweenObj(false , obj)));
    }

    // ------------------------------------------------------------------------- //
    // 돌리고 돌리고 돌리는 tween
    void RotateAnimation(GameObject obj,float speed , bool cw)
    {
        iTween.RotateBy(obj , iTween.Hash(
            "z" , (!cw) ? 100.0f : -100.0f ,
            "speed" , speed ,
            "oncompletetarget" , gameObject ,
            "oncomplete" , "RotateAnimationEnd" ,
            "oncompleteparams",new TweenObj(speed , obj,cw)));
    }

    // tween End -여어어어어엉원히 돌아야 한다
    void RotateAnimationEnd(TweenObj t)
    {
        RotateAnimation(t.target , t.speed , t.cw);
    }

    // ------------------------------------------------------------------------ //
    // '확' 커지고 사라짐
    void ScaleUpAnimation(GameObject obj,float speed = 8.0f)
    {
        iTween.ScaleTo(obj , iTween.Hash(
            "x" , 5.0f ,
            "y" , 5.0f ,
            "speed",speed,
            "easetype" , "easeOutExpo",
            "oncompletetarget",gameObject,
            "oncomplete","HideAnimation",
            "oncompleteparams",obj
            ));
    }

    void HideAnimation(GameObject target)
    {
        target.SetActive(false);
        PopupManager.Instance().ClosePopup(gameObject);
        GameManager.Instance().UIShow(true);
    }
}
