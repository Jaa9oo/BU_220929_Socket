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
    InputField ipSend;
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

        ipSend.OnEndEditAsObservable().Subscribe(x =>
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
        BtnConnect.gameObject.SetActive(!bConnected);
        ipSend.interactable = ipNickName.text != "";
        //���ν������ ��Ʈ��ũ �����尡 �и��Ǿ� �̷��� �ۼ��ؾ���
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
        if (ipSend.text == "")
            return;
        //buffer = new byte[1024];

        string msg = ipNickName.text == "" ? "NoName" : ipNickName.text;
        msg += " : " + ipSend.text;

        //msg ������ byte ������ ��ȯ
        //buffer = new byte[System.Text.ASCIIEncoding.ASCII.GetBytes(msg).Length];
        byte[] temp = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
        Array.Copy(temp, buffer, temp.Length);


        //���۸� ����
        client.BeginSend(buffer, 0, temp.Length, SocketFlags.None, SendCallback, null);


        ipSend.text = "";
    }

    public void Connect()
    {
        print("Ŭ���̾�Ʈ ������");


        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //ip�ּҷ� ������ �����ϵ��� ����
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

        print("������� : " + len);

        //client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, null);
    }

    void RecvCallback(IAsyncResult result)
    {
        //Socket client = (Socket)result.AsyncState;
        int len = client.EndReceive(result);

        if (len > 0)
        {
            string recv = System.Text.ASCIIEncoding.ASCII.GetString(this.buffer, 0, len);

            //fromNetThread��� ������ recv�� ����
            fromNetThread = recv;

            print(recv);
        }
        
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, null);
    }
}
