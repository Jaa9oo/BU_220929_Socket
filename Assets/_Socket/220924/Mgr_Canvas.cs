using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class Mgr_Canvas : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI txtUI_Client;
    [SerializeField]
    TextMeshProUGUI txtUI_Server;

    CompositeDisposable disposables;

    private void Start()
    {
        disposables = new CompositeDisposable();

        MessageBroker.Default.Receive<EVT_ReceveMsg>().Subscribe(evt =>
        {
            Debug.Log(evt.sMsg);
            switch (evt.rcvType)
            {
                case Type.Clinet:
                    txtUI_Client.text = txtUI_Client.text + "\n" + evt.sMsg;
                    break;
                case Type.Server:
                    txtUI_Server.text = txtUI_Server.text + "\n" + evt.sMsg;
                    break;
            }
        }).AddTo(disposables);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
