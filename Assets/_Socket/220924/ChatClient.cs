using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Random = UnityEngine.Random;

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

            // �޼��� ':'�� �������� ������ �����ϱ�
            string[] div = fromNetThread.Split(':');

            // �޼��������� ��ɾ� Ȯ��
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

        string msg = ipNickName.text == "" ? "NoName" : ipNickName.text;
        msg += " : " + ipSend.text;

        //msg ������ byte ������ ��ȯ
        //buffer = new byte[System.Text.ASCIIEncoding.ASCII.GetBytes(msg).Length];
        byte[] temp = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
        Array.Copy(temp, buffer, temp.Length);


        //���۸� ����
        client.BeginSend(buffer, 0, temp.Length, SocketFlags.None, SendCallback, null);


        ipSend.text = "";
        ipNickName.interactable = false;
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

    void Parser(string[] msg)
    {
        if (msg[1].Contains("create"))
        {
            if (GameObject.Find(msg[0]))
                return;
            var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.AddComponent<Mgr_Player>();
            player.transform.position = Vector3.zero;
            player.transform.rotation = Quaternion.identity;


            // �۽����� �̸����� �÷��̾� ����
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

    //void Parser(string[] msg)
    //{
    //    if (msg[1].Contains("create"))
    //    {
    //        if (GameObject.Find(msg[0]))
    //            return;
    //        var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        player.transform.position = Vector3.zero;
    //        player.transform.rotation = Quaternion.identity;

    //        // �÷��̾� ���� ���� ����
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

    //        // �۽����� �̸����� �÷��̾� ����
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
