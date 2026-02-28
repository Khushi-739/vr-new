using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Manager : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;

    public int port = 5052;
    public bool startReceiving = true;


    private readonly object dataLock = new object();
    private string _data;

    public string data
    {
        get
        {
            lock (dataLock)
            {
                return _data;
            }
        }
        private set
        {
            lock (dataLock)
            {
                _data = value;
            }
        }
    }

    void Start()
    {
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        client = new UdpClient(port);

        while (startReceiving)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);

                data = Encoding.UTF8.GetString(dataByte);
                Debug.Log("UDP: " + data);
            }
            catch (ThreadAbortException)
            {

                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    void OnApplicationQuit()
    {
        startReceiving = false;

        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Abort();

        if (client != null)
            client.Close();
    }
}
