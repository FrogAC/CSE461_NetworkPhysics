﻿using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
// This class provides the GUI for the game
public class ModeSelection : MonoBehaviour {
    bool isStarted = false;
    string addrstr = "addr";
    string portstr_h = "12345";
    string portstr_c = "12345";
    private int playerId = 0;

    // There are six players
    private string[] toolbarStrings = {"p1", "p2", "p3", "p4", "p5", "p6"};

    IPEndPoint localIP;
    Host host = null;
    Client client = null;

    void Start() {
        using(Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
            socket.Connect("127.0.0.1", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint;
            addrstr = localIP.Address.ToString();
        }
    }

    // Make several GUI options for the user to select as host or client
    // Also, there are more options such as changing force type
    // and close connections.
    void OnGUI() {
        if (!isStarted) {
            GUI.BeginGroup(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200));

            playerId = GUI.Toolbar (new Rect (0,100, 400, 40), playerId, toolbarStrings);

            if (GUI.Button(new Rect(10, 10, 180, 30), "As Host")) {
                Game.Instance.InitGame(playerId, true);
                host = gameObject.AddComponent<Host>();
                host.Init(localIP.Address.ToString(), Convert.ToInt32(portstr_h));
                isStarted = true;
            }
            GUI.Box(new Rect(10, 50, 130, 40), localIP.Address.ToString());
            portstr_h = GUI.TextField(new Rect(145, 50, 45, 40), portstr_h);

            if (GUI.Button(new Rect(200, 10, 180, 30), "As Client")) {
                Game.Instance.InitGame(playerId, false);
                client = gameObject.AddComponent<Client>();
                client.Init(addrstr, Convert.ToInt32(portstr_c));
                isStarted = true;
            }
            addrstr = GUI.TextField(new Rect(200, 50, 130, 40), addrstr);
            portstr_c = GUI.TextField(new Rect(335, 50, 45, 40), portstr_c);

            GUI.EndGroup();

        } else {
            if (host) {
                GUI.Box(new Rect(0, 0, 200, 40), "[Host] " + host.HostIP);
                if (GUI.Button(new Rect(50, 100, 200, 40),"Impulse Force on Cube")) {
                    Game.Instance.changeImpulseForceOnCube();
                }
            } else {
                GUI.Box(new Rect(0, 0, 200, 40), "[Client] " + client.HostIP);
                if (GUI.Button(new Rect(200, 200, 130, 40), "Close Connection")) {
                    client.Close();
                    isStarted = false;
                }
            }
        }
    }

}