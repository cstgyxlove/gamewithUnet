﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Net;
using System.Net.Sockets;

public class MenuControl : MonoBehaviour {

	public void StartLocalGame()
	{

    //    StartCoroutine(checkRoom());
	//}
	
 //   IEnumerator checkRoom()
 //   {
 //       int roomid = 0;
 //       int.TryParse(hostNameInput.text,out roomid);

 //       if(roomid != 0)
 //       {
 //           string ip = LocalIPAddress();
 //           string url = "59.111.96.26/host.php?ip="+ ip +"&roomid="+ roomid;
 //           Debug.Log(url);
 //           WWW www = new WWW(url);
 //           yield return www;
 //           Debug.Log(www.text);
 //       }
 //       yield return 0;
        NetworkManager.singleton.StartHost();
    }


    public string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }

    public void JoinLocalGame()
	{
		if (hostNameInput.text != "Hostname")
		{
            if (LocalIPAddress() == hostNameInput.text)
                hostNameInput.text = "127.0.0.1";

            NetworkManager.singleton.networkAddress = hostNameInput.text;
		}	
		NetworkManager.singleton.StartClient();
	}
	
	public void StartMatchMaker()
	{
		NetworkManager.singleton.StartMatchMaker();
	}
	
	public UnityEngine.UI.Text hostNameInput;


	void Start()
	{
       // hostNameInput.text = NetworkManager.singleton.networkAddress;
	}
	
}
