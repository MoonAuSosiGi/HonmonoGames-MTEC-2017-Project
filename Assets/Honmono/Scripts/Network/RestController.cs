using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class RestController : MonoBehaviour {

    [SerializeField]
    private string url = "localhost:8080";
	// Use this for initialization
	void Start () {
        //StartCoroutine(Get(gameObject, "TEST"));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [SerializeField]
    class LoginData
    {
        public string user;
        public string password;
    }

    public void Login(string id,string password,GameObject target,string targetFunc)
    {        
        LoginData data = new LoginData();
        data.user = id;
        data.password = password;

        string jsonData =  JsonUtility.ToJson(data);

        StartCoroutine(Post(jsonData, target, targetFunc));
        
    }

    IEnumerator Post(string json,GameObject target, string targetFunc)
    {

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.Send();

        if(www.isError)
        {
            MDebug.Log("REST Post Error : "+ www.error);
        }
        else
        {
            if(www.GetResponseHeaders().Count > 0)
            {
                MDebug.Log("response code " + www.responseCode);
                if (target != null)
                    target.SendMessage(targetFunc, this.GetJSON(www));
                
                
                MDebug.Log(www.downloadHandler.text);
                
                
            }
        }
    }

    IEnumerator Get(GameObject target, string targetFunc)
    {

        UnityWebRequest www = UnityWebRequest.Get(url + "/users");

        yield return www.Send();

        if(www.isError)
        {
            MDebug.Log("REST Get Error : " + www.error);
        }
        else
        {
            if (www.GetResponseHeaders().Count > 0) //.responseHeaders.Count > 0)
            {

                if (target != null)
                    target.SendMessage(targetFunc, this.GetJSON(www));
            }
        }
        
    }

    JSONObject GetJSON(UnityWebRequest www)
    {
        byte[] results = www.downloadHandler.data;
        JSONObject obj = new JSONObject(www.downloadHandler.text);
        MDebug.Log("d " + System.Convert.ToBase64String(results));
        obj.AddField("data", System.Convert.ToBase64String(results));
        //obj.AddField("status", www.downloadHandler.text);
        obj.AddField("responseCode", new JSONObject(www.responseCode));
        obj.AddField("responseHeader", new JSONObject(www.GetResponseHeaders()));
        return obj;
    }
}
