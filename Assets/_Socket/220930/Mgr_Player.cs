using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Mgr_Player : MonoBehaviour
{
    MeshRenderer mesh;
    CompositeDisposable disposables;

    bool bMoveFAct;
    bool bMoveUAct;
    bool bRotateAct;

    int nForward;
    int nRight;
    int nUp;

    Color[] colors = { Color.black, Color.blue, Color.cyan, Color.gray, Color.red, Color.green, Color.grey, Color.yellow, Color.white };
    int nColorIndex = 0;
    private void Start()
    {
        disposables = new CompositeDisposable();

        MessageBroker.Default.Receive<EVT_MoveForward>().Subscribe(evt =>
        {
            if (this.gameObject.name == evt.name)
            {
                bMoveFAct = !bMoveFAct;
                nForward = evt.isForward == true ? 1 : -1;
            }
        }).AddTo(disposables);

        MessageBroker.Default.Receive<EVT_MoveUp>().Subscribe(evt =>
        {
            if (this.gameObject.name == evt.name)
            {
                bMoveUAct = !bMoveUAct;
                nUp = evt.isUp == true ? 1 : -1;
            }
        }).AddTo(disposables);

        MessageBroker.Default.Receive<EVT_Rotate>().Subscribe(evt =>
        {
            if (this.gameObject.name == evt.name)
            {
                bRotateAct = !bRotateAct;
                nRight = evt.isRight == true ? 1 : -1;
            }
        }).AddTo(disposables);

        MessageBroker.Default.Receive<EVT_ChangeColor>().Subscribe(evt =>
        {
            if (this.gameObject.name == evt.name)
            {
                mesh.material.color = colors[nColorIndex++];
            }
        }).AddTo(disposables);

        mesh = GetComponent<MeshRenderer>();

        mesh.material.color = colors[Random.Range(0, colors.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        if (bMoveFAct)
        {
            transform.Translate(transform.forward * 5f * Time.deltaTime * nForward);
        }
        if (bRotateAct)
        {
            transform.Rotate(transform.up * 10f * Time.deltaTime * nRight);
        }
        if (bMoveUAct)
        {
            transform.Translate(transform.up * 5f * Time.deltaTime * nUp);
        }

        if (nColorIndex >= colors.Length)
            nColorIndex = 0;
    }
}
