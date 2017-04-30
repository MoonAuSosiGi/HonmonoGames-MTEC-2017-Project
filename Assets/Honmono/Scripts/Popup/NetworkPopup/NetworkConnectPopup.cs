using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkConnectPopup : MonoBehaviour {

    // -- 기본 세팅 -------------------------------------------------------------//
    [SerializeField]
    private InputField m_userNameField = null;
    [SerializeField]
    private InputField m_serverURL = null;

    [SerializeField]

    // -------------------------------------------------------------------------//

    public void ChangeText()
    {
        GameManager.Instance().PLAYER.USER_NAME = m_userNameField.text;
    }

	public void ServerConnect()
    {
        NetworkManager.Instance().SERVER_URL = m_serverURL.text;
        NetworkManager.Instance().SetupWebSocket();

        PopupManager.Instance().ClosePopup(gameObject);
    }
}
