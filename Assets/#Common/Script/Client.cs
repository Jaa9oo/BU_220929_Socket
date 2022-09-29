using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{

    Socket client;
    string fromNetThread = "";
    byte[] buffer = new byte[1024];

    void Start()
    {
        
    }

    void Update()
    {
        //메인스레드와 네트워크 스레드가 분리되어 이렇게 작성해야함
        if(fromNetThread.Length > 0)
        {
            GameObject.Find("UI Text").GetComponent<UnityEngine.UI.Text>().text += "\n" + fromNetThread;

            fromNetThread = "";
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("클라이언트 접속중");


            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
             
            //ip주소로 서버에 접속하도록 설정
            client.BeginConnect("127.0.0.1", 10000, null, client);

        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            string msg = GameObject.Find("InputField").GetComponent<UnityEngine.UI.InputField>().text;

            //msg 변수를 byte 단위로 변환
            buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);

            //버퍼를 보냄
            client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, null);
        }
    }

    void SendCallback(IAsyncResult result)
    {
        int len = client.EndSend(result);

        print("보낸결과 : " + len);

        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, null);
    }

    void RecvCallback(IAsyncResult result)
    {
        //Socket client = (Socket)result.AsyncState;
        int len = client.EndReceive(result);

        if (len > 0)
        {
            string recv = System.Text.ASCIIEncoding.ASCII.GetString(this.buffer);

            //fromNetThread라는 변수에 recv값 저장
            fromNetThread = recv;

            print(recv);
        }
    }
    }
