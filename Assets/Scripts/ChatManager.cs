using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class ChatManager : MonoBehaviourPun, IPunObservable
{

    public PhotonView photonView;
    public GameObject bubbleSpeechObject;
    public TMP_Text updatedTxt;
    private TMP_InputField chatInputField;
    private bool disableSend;

    void Start()
    {
        chatInputField = GameObject.Find("ChatInputField").GetComponent<TMP_InputField>();
    }

   
    void Update()
    {
        if(photonView.IsMine)
        {
            if(!disableSend && chatInputField.isFocused)
            {
                if(chatInputField.text != "" && chatInputField.text.Length >0 && Input.GetKeyDown(KeyCode.Slash))
                {
                    photonView.RPC("SendMessage", RpcTarget.AllBuffered, chatInputField.text);
                    bubbleSpeechObject.SetActive(true);
                    chatInputField.text = "";
                    disableSend = true;
                }
            }
        }
    }
    [PunRPC]
    public void SendMessage(string message)
    {
        updatedTxt.text = message;
        StartCoroutine(Remove());
    }
    IEnumerator Remove()
    {
        yield return new WaitForSeconds(4f);
        bubbleSpeechObject.SetActive(false);
        disableSend = false;
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(bubbleSpeechObject.active);
        }
        else if(stream.IsReading)
        {
            bubbleSpeechObject.SetActive((bool)stream.ReceiveNext());
        }
    }
    
}
