using System.Collections;
using System.Collections.Generic;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Text.RegularExpressions;
using TMPro;

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

    //-------------------------------------------------------------------------
    private void HandleMessage(string message)
    {
        //Debug.Log(message+'\n'+message.Substring(1,(message.Length-2)));
        Debug.Log("Inside Handle Message, Recieving Data\n");
        //string[] liness = Regex.Split(message, "\n");
        var liness = new List<string>(Regex.Split(message, "\n"));
        List<Vector3> ips = new List<Vector3>();

        //Debug.Log("Count Liness is  "+ liness.Count);  //checked
        foreach (string l in liness)
        {
            //print(l);
            var splittedStrings = l.Substring(1, l.Length - 2).Split(' ');
            //var splittedStrings = l.Split(' ');

            List<string> splitstringnew = new List<string>(splittedStrings);
            splitstringnew.RemoveAll(str => string.IsNullOrEmpty(str));
            Debug.Log("FOR EACH LOOP, the length of string   " + ips.Count);
            if (splitstringnew.Count != 3) return;
        
            var x = float.Parse(splitstringnew[0]);
            var y = float.Parse(splitstringnew[1]);
            var z = float.Parse(splitstringnew[2]);
            ips.Add(new Vector3(x, y, z));
        }
        Debug.Log("printing the arrow1!!!!!!!!!!!!");
        var xyz = DrawPCAComponentsXYZ(ips[0], ips.GetRange(1, ips.Count - 1));
        Debug.Log("returned");
        //Destroy(xyz);


    }
    public GameObject DrawPCAComponentsXYZ(Vector3 origin, List<Vector3> endpoints)
    {
        Debug.Log("Creating arrows...\n");
        var ct = 1;
        var parent_obj = new GameObject();
        foreach (var endpoint in endpoints)
        {
            var arrow = DrawArrow(origin, endpoint, "PCA" + ct);
            arrow.transform.SetParent(parent_obj.transform);
            ct += 1;
        }
        return parent_obj;
    }

    public GameObject DrawArrow(Vector3 origin, Vector3 vectorEndpoint, string principalComponentName)
    {
        var lineObject = new GameObject();
        lineObject.transform.SetParent(transform);
        var textObject = new GameObject();
        textObject.transform.SetParent(lineObject.transform);
        LineRenderer lineRenderer = lineObject.gameObject.AddComponent<LineRenderer>();
        TextMeshPro endpointText = textObject.gameObject.AddComponent<TextMeshPro>();
        endpointText.fontSize = 3.0f;
        endpointText.text = $"{principalComponentName}\n{vectorEndpoint.ToString()}";
        endpointText.alignment = TextAlignmentOptions.Center;
        endpointText.transform.position = vectorEndpoint + vectorEndpoint.normalized * 0.5f;
        endpointText.color = Color.white;
        lineRenderer.SetWidth(0.01F, 0.04F);
        lineRenderer.SetVertexCount(2);
        Vector3[] positionArray = new[] { origin, vectorEndpoint };
        lineRenderer.SetPositions(positionArray);
        return lineObject;
    }


    //-------------------------------------------------------------------------

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


}
