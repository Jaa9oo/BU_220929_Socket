using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.IO;

public class ChatClient : MonoBehaviour
{
    [SerializeField]
    InputField ipSend;
    [SerializeField]
    InputField ipNickName;
    [SerializeField]
    RawImage rawImg;


    [SerializeField]
    Button BtnConnect;

    Socket client;
    string fromNetThread = "";
    byte[] buffer = new byte[1024];
    byte[] imgdata;
    bool bConnected;

    string host = "127.0.0.1";
    int port = 65432;

    string imgpath;

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
        ipNickName.interactable = bConnected;
        ipSend.interactable = ipNickName.text != "";
        //메인스레드와 네트워크 스레드가 분리되어 이렇게 작성해야함
        if (fromNetThread.Length > 0)
        {
            // 이미지 송신 준비 완료 이벤트 서버로부터 수신 확인
            if (fromNetThread.Contains("LoadImgOk"))
            {
                // 이미지 서버 전송
                client.BeginSend(imgdata, 0, imgdata.Length, SocketFlags.None, SendCallback, null);
                fromNetThread = "";
                return;
            }
            // 메세지 ':'를 기준으로 나누어 저장하기
            string[] div = fromNetThread.Split(';');

            // 메세지에서의 명령어 확인
            Parser(div);

            MessageBroker.Default.Publish(new EVT_ReceveMsg()
            {
                rcvType = Type.Clinet,
                sMsg = "[" + DateTime.Now + "]" + fromNetThread
            });

            fromNetThread = "";
        }
        KeyboardInput();
    }

    void KeyboardInput()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                ipSend.text = "create";
                SendMsg();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ipSend.text = "reset";
                SendMsg();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                ipSend.text = "delete";
                SendMsg();
            }
        }

        //if (Input.GetKey(KeyCode.UpArrow))
        //{
        //    ipSend.text = "forward";
        //    SendMsg();
        //}
        //if (Input.GetKey(KeyCode.DownArrow))
        //{
        //    ipSend.text = "back";
        //    SendMsg();
        //}
        // if (Input.GetKey(KeyCode.LeftArrow))
        //{
        //    ipSend.text = "left";
        //    SendMsg();
        //}
        //if (Input.GetKey(KeyCode.RightArrow))
        //{
        //    ipSend.text = "right";
        //    SendMsg();
        //}

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ipSend.text = "forward";
            SendMsg();
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            ipSend.text = "back";
            SendMsg();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ipSend.text = "left";
            SendMsg();
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            ipSend.text = "right";
            SendMsg();
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            ipSend.text = "up";
            SendMsg();
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            ipSend.text = "down";
            SendMsg();
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            ipSend.text = "forward";
            SendMsg();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            ipSend.text = "back";
            SendMsg();
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            ipSend.text = "left";
            SendMsg();
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            ipSend.text = "right";
            SendMsg();
        }
        if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            ipSend.text = "up";
            SendMsg();
        }
        if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            ipSend.text = "down";
            SendMsg();
        }

    }

    void SendMsg()
    {
        if (!bConnected)
            return;
        if (ipSend.text == "")
            return;
        //buffer = new byte[1024];
        string msgtotal = "";

        // 아이디 없으면 노네임으로(사용x)
        var msg1 = ipNickName.text == "" ? "NoName" : ipNickName.text;
        
        // 내용에 이미지 전송 명령어 확인하여 구분
        var msg2 = ipSend.text.Contains("LoadImg") ?  "" : ipSend.text;
        
        // 이미지 명령 아닐경우
        if (msg2 != "")
        {
            msgtotal = msg1 + " ; " + msg2;
            //msg 변수를 byte 단위로 변환
            //buffer = new byte[System.Text.ASCIIEncoding.ASCII.GetBytes(msg).Length];
            byte[] temp = System.Text.ASCIIEncoding.ASCII.GetBytes(msgtotal);


            Array.Copy(temp, buffer, temp.Length);
            //버퍼를 보냄
            client.BeginSend(buffer, 0, temp.Length, SocketFlags.None, SendCallback, null);
        }

        // 이미지 명령일경우
        else
        {
            imgpath = ipSend.text.Split('_')[1];
            imgdata = LoadByteImg(imgpath);

            // 이미지 전송 명령어 뒤에 바이트 크기 전송
            msgtotal = "LoadImg_"+ imgdata.Length;
            byte[] temp = System.Text.ASCIIEncoding.ASCII.GetBytes(msgtotal);
            

            Array.Copy(temp, buffer, temp.Length);
            //버퍼를 보냄
            client.BeginSend(buffer, 0, temp.Length, SocketFlags.None, SendCallback, null);
            

            ////버퍼를 보냄
            //byte[] imgTemp = LoadByteImg(ipSend.text.Split('_')[1]);
            //client.BeginSend(imgTemp, 0, imgTemp.Length, SocketFlags.None, SendCallback, null);
            //Debug.Log(imgTemp.Length);
        }

        ipSend.text = "";
        ipNickName.interactable = false;
    }

    public void Connect()
    {
        print("클라이언트 접속중");


        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //ip주소로 서버에 접속하도록 설정
        client.BeginConnect(host, port, ConnectCallBack, client);
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
            string recv = System.Text.ASCIIEncoding.ASCII.GetString(this.buffer, 0, len);

            //fromNetThread라는 변수에 recv값 저장
            fromNetThread = recv;

          

            print(recv);
        }

        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecvCallback, null);
    }

    void Parser(string[] msg)
    {
        if (msg.Length < 2)
            return;
        if (msg[1].Contains("create"))
        {
            if (GameObject.Find(msg[0]))
                return;
            var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.AddComponent<Mgr_Player>();
            player.transform.position = Vector3.zero;
            player.transform.rotation = Quaternion.identity;


            // 송신자의 이름으로 플레이어 생성
            player.name = msg[0];
        }
        else if (msg[1].Contains("forward"))
        {
            MessageBroker.Default.Publish(new EVT_MoveForward()
            {
                name = msg[0],
                isForward = true
            });
        }
        else if (msg[1].Contains("back"))
        {
            MessageBroker.Default.Publish(new EVT_MoveForward()
            {
                name = msg[0],
                isForward = false
            });
        }
        else if (msg[1].Contains("up"))
        {
            MessageBroker.Default.Publish(new EVT_MoveUp()
            {
                name = msg[0],
                isUp = true
            });
        }
        else if (msg[1].Contains("down"))
        {
            MessageBroker.Default.Publish(new EVT_MoveUp()
            {
                name = msg[0],
                isUp = false
            });
        }
        else if (msg[1].Contains("left"))
        {
            MessageBroker.Default.Publish(new EVT_Rotate()
            {
                name = msg[0],
                isRight = false
            });
        }
        else if (msg[1].Contains("right"))
        {
            MessageBroker.Default.Publish(new EVT_Rotate()
            {
                name = msg[0],
                isRight = true
            });
        }
        else if (msg[1].Contains("reset"))
        {
            var player = GameObject.Find(msg[0]);
            if (player != null)
            {
                player.transform.position = Vector3.zero;
                player.transform.rotation = Quaternion.identity;
            }
        }
        else if (msg[1].Contains("delete"))
        {
            var player = GameObject.Find(msg[0]);
            if (player != null)
                Destroy(player);
        }
        else if (msg[1].Contains("change"))
        {
            MessageBroker.Default.Publish(new EVT_ChangeColor()
            {
                name = msg[0],
            });
        }
    }


    /// <summary>
    /// 이미지 변환
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    byte[] LoadByteImg(string filepath)
    {
        byte[] filedata;

        filedata = File.ReadAllBytes(filepath);

        LoadImg(filedata);

        return filedata;
    }

    string ByteArrayToString(byte[] val)
    {
        string b = "";
        int len = val.Length;
        for (int i = 0; i < len; i++)
        {
            if (i != 0)
            {
                b += ",";
            }
            b += val[i].ToString();
        }
        return b;
    }

    /// <summary>
    /// 이미지 로딩
    /// </summary>
    /// <param name="filedata"></param>
    /// <returns></returns>
    public Texture2D LoadImg(byte[] filedata)
    {
        Texture2D tex = null;

        tex = new Texture2D(2, 2);
        tex.LoadImage(filedata);

        rawImg.texture = tex;
        return tex;
    }

    //void Parser(string[] msg)
    //{
    //    if (msg[1].Contains("create"))
    //    {
    //        if (GameObject.Find(msg[0]))
    //            return;
    //        var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        player.transform.position = Vector3.zero;
    //        player.transform.rotation = Quaternion.identity;

    //        // 플레이어 색상 랜덤 변경
    //        while(true)
    //        {
    //            var _index = Random.Range(0, colors.Length);
    //            if (nIndexes.Contains(_index))
    //                continue;
    //            else
    //            {
    //                nIndexes.Add(_index);
    //                break;
    //            }
    //        }




    //        player.GetComponent<MeshRenderer>().material.color = colors[nIndexes[nIndexes.Count - 1]];

    //        // 송신자의 이름으로 플레이어 생성
    //        player.name = msg[0];
    //    }
    //    else if (msg[1].Contains("forward"))
    //    {
    //        var player = GameObject.Find(msg[0]);
    //        if (player != null)
    //            player.transform.Translate(player.transform.forward * 5f * Time.deltaTime);
    //            //player.transform.position += new Vector3(0, 0, 1);
    //    }
    //    else if (msg[1].Contains("back"))
    //    {
    //        var player = GameObject.Find(msg[0]);
    //        if (player != null)
    //            player.transform.Translate(player.transform.forward * -5f * Time.deltaTime);
    //        //player.transform.position += new Vector3(0, 0, -1);
    //    }
    //    else if (msg[1].Contains("left"))
    //    {
    //        var player = GameObject.Find(msg[0]);
    //        if (player != null)
    //            player.transform.Rotate(player.transform.up * -10f * Time.deltaTime);
    //    }
    //    else if (msg[1].Contains("right"))
    //    {
    //        var player = GameObject.Find(msg[0]);
    //        if (player != null)
    //            player.transform.Rotate(player.transform.up * 10f * Time.deltaTime);
    //    }
    //    else if (msg[1].Contains("reset"))
    //    {
    //        var player = GameObject.Find(msg[0]);
    //        if (player != null)
    //        {
    //            player.transform.position = Vector3.zero;
    //            player.transform.rotation = Quaternion.identity;
    //        }
    //    }
    //    else if (msg[1].Contains("delete"))
    //    {
    //        var player = GameObject.Find(msg[0]);
    //        if (player != null)
    //            Destroy(player);
    //    }

    //    MessageBroker.Default.Publish(new EVT_ReceveMsg()
    //    {
    //        rcvType = Type.Clinet,
    //        sMsg = "[" + DateTime.Now + "]" + fromNetThread
    //    });
    //}
}
