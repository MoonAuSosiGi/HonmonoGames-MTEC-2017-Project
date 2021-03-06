﻿using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPopup : MonoBehaviour ,PopupManager.PopupHide{

    // -- 로그인 팝업 ----------------------------//
    [SerializeField]
    private InputField m_inputID = null;
    [SerializeField]
    private InputField m_inputPWD = null;


    [SerializeField]
    private Button m_loginButton = null;

    [SerializeField]
    private RectTransform m_loadingHide = null;
    [SerializeField]
    private RectTransform m_loading = null;

    //로고 바꿔야함
  //  [SerializeField]
 //   private Image m_lockImg = null;

    [SerializeField]
    private Sprite m_unlockImg = null;

    private bool m_socketCheck = false;

    public InputField m_serverInput = null;
    //-------------------------------------------//


	// Use this for initialization
	void Start () {
        //  SoundManager.Instance().PlayBGM(0);
        m_serverInput.text = "localhost"; //  "52.79.58.106";
	}
	
	// Update is called once per frame
	void Update () {
		

        if(m_socketCheck && NetworkManager.Instance().IsSocketAlived())
        {
            StopAllCoroutines();
         //   m_lockImg.sprite = m_unlockImg;

            if(!IsInvoking())
                Invoke("LoginGO", 0.5f);
            return;
        }

    }

    void LoginGO()
    {
        //CameraManager.Instance().MoveCamera(
        //    CameraManager.Instance().m_gameStart , 10.0f , CameraManager.CAMERA_PLACE.GAME_START,
        //    Camera.main.gameObject,"TitleEnd");
        Camera.main.gameObject.SendMessage("TitleEnd");
        m_socketCheck = false;
        
        PopupManager.Instance().ClosePopup(gameObject);
    }

    public void LoginButton()
    {
        string id = m_inputID.text;
        string pwd = m_inputPWD.text;

        NetworkManager.Instance().SERVER_URL = m_serverInput.text;

        m_loading.transform.parent.gameObject.SetActive(true);
        m_loadingHide.gameObject.SetActive(false);

        NetworkManager.Instance().Login(id, pwd, gameObject, "RecvMessage");
        StartCoroutine(Loading());

    }

    void LoginProcess()
    {
        NetworkManager.Instance().SetupWebSocket();
        m_socketCheck = true;
    }

    void RecvMessage(JSONObject obj)
    {
        if(!obj.HasField("status"))
        {
            PopupManager.Instance().ClosePopup(gameObject);
            PopupManager.Instance().MessagePopupOK("ERROR", "REST 서버에 접속할 수 없습니다.");
            PopupManager.Instance().AddPopup("LoginPopup");
            return;
        }

        // RESPONSE CODE 
        int response = (int)obj.GetField("responseCode").n;
        // HEADER
        JSONObject header = obj.GetField("responseHeader");
        //result
        string result = obj.GetField("status").str;

        //MDebug.Log("responseCode " + response + " header " + header + " result " + result);

        //Success
        if(response == 200)
        {
            //TODO TEMP CODE
            if(result == "you are signed up.")
            {
                
                GameManager.Instance().PLAYER.USER_NAME = m_inputID.text;
                Thread t = new Thread((LoginProcess));

                t.Start();
            }
            else
            {
                //DENY 애니메이션
                m_loading.transform.parent.gameObject.SetActive(true);
                m_loadingHide.gameObject.SetActive(true);
            }
        }

    }

    IEnumerator Loading()
    {
        yield return new WaitForSeconds(0.01f);
        m_loading.Rotate(new Vector3(0, 0, 0.5f));
        yield return StartCoroutine(Loading());

    }

    public void HideEndEvent()
    {
        if(GameManager.Instance().PLAYER.NETWORK_INDEX > 1)
            GameManager.Instance().ChangeScene(GameManager.PLACE.ONLY_POPUP_SHOW);
        else
        {
            SoundManager.Instance().PlayBGM(3);

            GameManager.Instance().ChangeScene(GameManager.PLACE.TUTORIAL_START);
        }
        
    }
}
