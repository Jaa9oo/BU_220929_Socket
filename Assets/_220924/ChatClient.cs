using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ChatClient : MonoBehaviour
{
    [SerializeField]
    InputField BtnSend;
    [SerializeField]
    InputField ipNickName;


    [SerializeField]
    Button BtnConnect;

    Socket client;
    string fromNetThread = "";
    byte[] buffer = new byte[1024];

    bool bConnected;

    CompositeDisposable disposables;
    private void Start()
    {
        disposables = new CompositeDisposable();

        BtnSend.OnEndEditAsObservable().Subscribe(x =>
        {
            Debug.Log("Unirx test send");
            SendMsg();
        }).AddTo(disposables);

        BtnConnect.OnClickAsObservable().Subscribe(x =>
        {
            Debug.Log("Unirx test connect");
            Connect();
        }).AddTo(disposables);
    }

    void Update()
    {
        BtnConnect.interactable = !bConnected;
        //메인스레드와 네트워크 스레드가 분리되어 이렇게 작성해야함
        if (fromNetThread.Length > 0)
        {
            MessageBroker.Default.Publish(new EVT_ReceveMsg()
            {
                rcvType = Type.Clinet,
                sMsg = "[" + DateTime.Now + "]" + fromNetThread
            });

            fromNetThread = "";
        }
    }

    void SendMsg()
    {
        if (!bConnected)
            return;
        if (BtnSend.text == "")
            return;
        //buffer = new byte[1024];

        string msg = ipNickName.text == "" ? "NoName" : ipNickName.text;
        msg += " : " + BtnSend.text;

        //msg 변수를 byte 단위로 변환
        //buffer = new byte[System.Text.ASCIIEncoding.ASCII.GetBytes(msg).Length];
        buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);



        //버퍼를 보냄
        client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, null);

        
    }

    public void Connect()
    {
        print("클라이언트 접속중");


        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //ip주소로 서버에 접속하도록 설정
        client.BeginConnect("127.0.0.1", 10000, ConnectCallBack, client);
    }

    void ConnectCallBack(IAsyncResult result)
    {
        bConnected = true;
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, null);
    }

    void SendCallback(IAsyncResult result)
    {
        int len = client.EndSend(result);

        print("보낸결과 : " + len);

        //client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, null);
    }

    void RecvCallback(IAsyncResult result)
    {
        //Socket client = (Socket)result.AsyncState;
        int len = client.EndReceive(result);

        if (len > 0)
        {
            string recv = System.Text.ASCIIEncoding.ASCII.GetString(this.buffer, 0, this.buffer.Length);

            //fromNetThread라는 변수에 recv값 저장
            fromNetThread = recv;

            print(recv);
        }
        
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, null);
    }
}
