using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPopup : MonoBehaviour {

    // -- 로그인 팝업 ----------------------------//
    [SerializeField]
    private InputField m_inputID = null;
    [SerializeField]
    private InputField m_inputPWD = null;
    //-------------------------------------------//


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoginButton()
    {
        string id = m_inputID.text;
        string pwd = m_inputPWD.text;

        NetworkManager.Instance().Login(id, pwd, gameObject, "RecvMessage");
    }

    void RecvMessage(JSONObject obj)
    {
        if(!obj.HasField("status"))
        {
            PopupManager.Instance().ClosePopup(gameObject);
            PopupManager.Instance().MessagePopupOK("ERROR", "REST 서버에 접속할 수 없습니다.");
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
                PopupManager.Instance().ClosePopup(gameObject);
                PopupManager.Instance().MessagePopupOK("Login", "완료");
            }
        }

    }
}
