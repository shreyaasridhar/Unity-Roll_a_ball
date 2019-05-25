﻿using System.Collections;
using System.Collections.Generic;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Threading;


public class NetMqListener
{

    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect("tcp://localhost:12345");
            subSocket.Subscribe("");
            while (!_listenerCancelled)
            {
                string frameString;
                if (!subSocket.TryReceiveFrameString(out frameString)) continue;
                Debug.Log(frameString);
                _messageQueue.Enqueue(frameString);
            }
            subSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    public void Update()
    {
        while (!_messageQueue.IsEmpty)
        {
            string message;
            if (_messageQueue.TryDequeue(out message))
            {
                _messageDelegate(message);
            }
            else
            {
                break;
            }
        }
    }

    public NetMqListener(MessageDelegate messageDelegate)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}


public class PCA_arrows : MonoBehaviour
{
    private NetMqListener _netMqListener;
    private Transform waypointArrow; //Transform used to reference the Waypoint Arrow
    private Transform currentWaypoint; //Transforms used to identify the Waypoint Arrow's target
    private Transform arrowTarget;
    [Range(0.0001f, 20)] public float arrowTargetSmooth; // controls how fast the arrow should smoothly target the next waypoint

    private void HandleMessage(string message)
    {
        Debug.Log(message+'\n'+message.Substring(1,(message.Length-2)));
        while (message.IndexOf('[') == 0) {
            var splittedStrings = message.Split(' ');
            if (splittedStrings.Length != 3) return;
            var x = float.Parse(splittedStrings[0]);
            var y = float.Parse(splittedStrings[1]);
            var z = float.Parse(splittedStrings[2]);
            //transform.position = new Vector3(x, y, z);
            //Keep the Waypoint Arrow pointed at the Current Waypoint
            if (arrowTarget != null)
            {
                arrowTarget.localPosition = Vector3.Lerp(arrowTarget.localPosition, currentWaypoint.localPosition, arrowTargetSmooth * Time.deltaTime);
                arrowTarget.localRotation = Quaternion.Lerp(arrowTarget.localRotation, currentWaypoint.localRotation, arrowTargetSmooth * Time.deltaTime);
            }
            else
            {
                arrowTarget = currentWaypoint;
            }
            if (waypointArrow == null)
                FindArrow();
            waypointArrow.LookAt(arrowTarget);
        }

    }

    private void Start()
    {
        _netMqListener = new NetMqListener(HandleMessage);
        _netMqListener.Start();
    }

    private void Update()
    {
        _netMqListener.Update();
    }

    private void OnDestroy()
    {
        _netMqListener.Stop();
    }
    public void CreateArrow()
    {
        GameObject instance = Instantiate(Resources.Load("Waypoint Arrow", typeof(GameObject))) as GameObject;
        instance.name = "Waypoint Arrow";
        instance = null;
    }

    public void FindArrow()
    {
        GameObject arrow = GameObject.Find("Waypoint Arrow");
        if (arrow == null)
        {
            CreateArrow();
            waypointArrow = GameObject.Find("Waypoint Arrow").transform;
        }
        else
        {
            waypointArrow = arrow.transform;
        }
    }

}
