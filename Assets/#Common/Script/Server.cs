using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Socket을 열기위함
using System;
using System.Net;
using System.Net.Sockets;

public class Server : MonoBehaviour
{

    Socket server;
    byte[] buffer = new byte[1024];

    void Start()
    {
        //Socket 세팅을 위한 설정
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        //로컬서버 10000번 포트 연결
        server.Bind(new IPEndPoint(IPAddress.Any, 10000));

        //접속을 위해 버퍼 사이즈를 10으로 설정
        server.Listen(10);


        //Callback 함수 선언(비동기)
        server.BeginAccept(AcceptCallback, server);
        print("서버 실행");
    } 



    void AcceptCallback(IAsyncResult result)
    {
        //클라이언트와 연결된 Socket
        Socket client = server.EndAccept(result);
        IPEndPoint addr = ((IPEndPoint)client.RemoteEndPoint);

        //Client의 정보도 가져옴
        print(string.Format("{0}, {1}", addr.ToString(), addr.Port.ToString()));

        //0부터 배열의 크기만큼 버퍼를 채움
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, client);

        print("pending");
    }

    void RecvCallback(IAsyncResult result)
    {
        Socket client = (Socket)result.AsyncState;
        //비동기적으로 데이터를 받는것을 멈춰야함

        int len = client.EndReceive(result);


        if (len > 0)
        {
            //len이 0보다 클때 버퍼에 있는걸 가져와 문자로 바꿔줌
            string recv = System.Text.ASCIIEncoding.ASCII.GetString(this.buffer);

            print(recv);

           client.BeginSend(buffer, 0, len, 
              SocketFlags.None, SendCallback, client);
        }

        //받은 버퍼를 서버에서 다시 돌려줌(에코서버 구성)
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, client);
    }


    void SendCallback(IAsyncResult result)
    {
        Socket client = (Socket)result.AsyncState;
        int len = client.EndSend(result);

        print("서버에서 보낸결과 : " + len);
    }


    void Update()
    {
        
    }
}
