﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Specialized;
using UnityEngine;
using Jh_Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ClientNet : MonoBehaviour
{
    private Socket mSocket;

    public delegate void mDelegateType(StructRequest msg);
    mDelegateType mCallback;
    
    bool mIsReceiving = false; // send-receive 사이클이 끝나야 또 send 가능
    bool mIsResponsing = false; // 응답이 왔을 경우


    bool mIsUdp = true;

    // 받은 응답 저장
    StructRequest mResponse;
    EndPoint mRemoteIpPoint;
    IPEndPoint mIpPoint;
    public ClientNet()
    {
        if (!mIsUdp)
        {
            IPEndPoint ip_point = new IPEndPoint(IPAddress.Parse("121.163.153.46"), 9999); // 서버 IP/Port
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            mSocket.Connect(ip_point);
            Debug.Log("Socket Connect");

            Thread p1 = new Thread(new ThreadStart(OnReceving));
            p1.Start();
        }
        else
        {


            mIpPoint = new IPEndPoint(IPAddress.Parse("121.163.153.46"), 8888); // 서버 IP/Port
            mRemoteIpPoint = new IPEndPoint(IPAddress.None, 9992);
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            mSocket.Connect(mIpPoint);
            Debug.Log("Socket Connect");
            Thread p1 = new Thread(new ThreadStart(OnReceving));
            p1.Start();

        }

    }

    void OnReceving()
    {
        while (true)
        {
            byte[] data = new byte[1024*3];

            if (!mIsUdp)
                mSocket.Receive(data);
            else
                mSocket.ReceiveFrom(data, ref mRemoteIpPoint);
            
            String msg = Encoding.Default.GetString(data);
            StructRequest request = NetFormatHelper.StringToStructRequest(msg);
            Debug.Log("수신 : " + msg);
            

            mResponse = request;
            mIsResponsing = true;
            mIsReceiving = false;
        }
        mSocket.Close();
    }

    // @Warning 유니티 싱글 스레드에서 동작하기 때문에 통신 스레드에서 받은 데이터를 여기 담아 사용
    private void Update()
    {
        if (mIsResponsing)
        {
            if (mCallback != null)
                mCallback(mResponse);

            mIsResponsing = false;

        }
    }

    public void RequestMsg(StructRequest request)
    {
        if (mIsReceiving) return;
        string msg = NetFormatHelper.StructRequestToString(request);
        Debug.Log("송신 : " + msg);

        byte[] data = NetFormatHelper.StringToByte(msg);

        if (!mIsUdp)
            mSocket.Send(data);
        else
            mSocket.SendTo(data, mIpPoint);
        mIsReceiving = true;
    }

    public void Close()
    {
        mSocket.Close();
    }


    void OnApplicationQuit()
    {
        mSocket.Close();
    }

    public void SetCallBack(mDelegateType call_back)
    {
        mCallback = call_back;
    }

}
