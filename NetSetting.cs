﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jh_Lib;
using UnityEngine.SceneManagement;

public class NetSetting : MonoBehaviour
{
    ClientNet mNet;
    StructUserData mUserData;
    public GameObject aaa;
    void Awake()
    {
        SetSocket();
    }

    void SetSocket()
    {
        // 소켓 설치
        gameObject.AddComponent<ClientNet>();
        mNet = GetComponent<ClientNet>();

        // @request UID 발급 신청
        StructRequest request = new StructRequest();
        request.uid = "";
        request.request_url = URL.InitUser.ToString();

        void CallBack(StructRequest response)
        {
            if (response.uid != null)
            {
                mUserData = new StructUserData();
                mUserData.uid = response.uid;
                mUserData.isPVPMode = true;
                string map_data = response.parameter["mapData"];
                string item_data = response.parameter["itemData"];

                // 맵 데이터에 저장
                JsonFactory.WriteString(Const.MapDataName, map_data);
                mUserData.itemData = item_data;
            }
        }

        mNet.SetCallBack(CallBack);
        mNet.RequestMsg(request);
    }

    // PVP
    public void GotoPVPGame()
    {
        // @request 상대 UID 요청
        StructRequest request = new StructRequest();
        request.uid = mUserData.uid;
        request.request_url = URL.GetOpponentData.ToString();

        void mCallBack(StructRequest response)
        {
            if (response.parameter != null)
            {
                if (response.parameter["opponentUid"] != null)
                {
                    StructUserData user_data = mUserData;
                    user_data.opponentUid = response.parameter["opponentUid"];

                    // 발급받은 UID는 로컬 데이터에 저장
                    JsonFactory.Write(Const.UserDataName, user_data);

                    // 상대 클라이언트가 준비되었다면 게임 실행
                    SceneManager.LoadScene("GameScene");
                }

            }
        }
        mNet.SetCallBack(mCallBack);
        mNet.RequestMsg(request);
    }


}
