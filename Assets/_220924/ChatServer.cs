using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;


public class ChatServer : MonoBehaviour
{


    Socket server;
    byte[] buffer = new byte[1024];
    List<Socket> clients = new List<Socket>();
    string fromNetThread = "";

    CompositeDisposable disposables;

    void Awake()
    {
        SetServer();
    }

    private void Start()
    {
        disposables = new CompositeDisposable();
        //MessageBroker.Default.Receive<EVT_ReceveMsg>()(evt =>
        //{
        //    Debug.Log("Unirx test send");
        //    SendMsg();
        //});

       
    }

    void SetServer()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        server.Bind(new IPEndPoint(IPAddress.Any, 10000));

        server.Listen(10);

        server.BeginAccept(AcceptCallback, null);
    }

    void AcceptCallback(IAsyncResult result)
    {
        //클라이언트와 연결된 Socket
        Socket client = server.EndAccept(result);
        IPEndPoint addr = ((IPEndPoint)client.RemoteEndPoint);

        //Client의 정보도 가져옴
        print(string.Format("{0}, {1}", addr.ToString(), addr.Port.ToString()));

        //SendEvent(string.Format("{0}, {1}", addr.ToString(), addr.Port.ToString()));

        //0부터 배열의 크기만큼 버퍼를 채움
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, client);

        clients.Add(client);

        print("pending");

        server.BeginAccept(AcceptCallback, null);
    }

    void RecvCallback(IAsyncResult result)
    {
        Socket client = (Socket)result.AsyncState;
        //비동기적으로 데이터를 받는것을 멈춰야함

        int len = client.EndReceive(result);


        if (len > 0)
        {
            //var recv = System.Text.Encoding.Default.GetString(buffer, 0, len);
            //buffer = new byte[len];
            //len이 0보다 클때 버퍼에 있는걸 가져와 문자로 바꿔줌
            string recv = System.Text.ASCIIEncoding.ASCII.GetString(this.buffer, 0, len);

            IPEndPoint addr = ((IPEndPoint)client.RemoteEndPoint);


            fromNetThread = string.Format("{0}, {1}", addr.Address, recv);



            //GameObject.Find("UI Text").GetComponent<Text>().text = string.Format("{0}, {1}", addr.Address, recv);

            //print(recv);

            // 동기
            //for (int i = 0; i < clients.Count; i++)
            //    clients[i].Send(buffer);

            // 비동기
            for (int i = 0; i < clients.Count; i++)
                clients[i].BeginSend(buffer, 0, len, SocketFlags.None, SendCallback, client);
        }

        //받은 버퍼를 서버에서 다시 돌려줌(에코서버 구성)
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, client);
    }

    private void Update()
    {
        if (fromNetThread.Length > 0)
        {
            MessageBroker.Default.Publish(new EVT_ReceveMsg()
            {
                rcvType = Type.Server,
                sMsg = "[" + DateTime.Now + "]" + fromNetThread
            });

            fromNetThread = "";
        }
    }

    //void SendEvent(string _data)
    //{
    //    MessageBroker.Default.Publish(new EVT_ReceveMsg()
    //    {
    //        rcvType = Type.Server,
    //        sMsg = "[Join - " + DateTime.Now + "]" + _data
    //    });
    //}

    void SendCallback(IAsyncResult result)
    {
        Socket client = (Socket)result.AsyncState;
        int len = client.EndSend(result);

        //print("서버에서 보낸결과 : " + len);
    }
}
