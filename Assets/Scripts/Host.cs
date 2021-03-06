using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * The host should start hosting a game session.
 * It should manage the game state by sending and 
 * receiving from any clients that connects.
 */
public class Host : MonoBehaviour {

    const float c_interval = 1f/30f;
    float _timeUntilNextUpdate = 0.0f;

    public string HostIP { get => serverSocket.Socket.LocalEndPoint.ToString(); }

    private string sAddr;
    private int sPort;

    private UDPSocket serverSocket;

    private Dictionary<EndPoint, Queue<byte[]>> ep2msg;
    private int lastClientSize = -1;
    private Snapshot _snapshot;

    void Start() {
        Assert.IsNull(FindObjectOfType<Client>());
    }

    public void Init(string address, int port) {
        sAddr = address;
        sPort = port;

        // endpoint to message dictionary
        ep2msg = new Dictionary<EndPoint, Queue<byte[]>>();
        serverSocket = new UDPSocket(ep2msg);
        serverSocket.Server(sAddr, sPort);

        _snapshot = Game.Instance.Snapshot.Clone();
    }

    // When any clients or host's game state changes,
    // the host will arrange to check about the snapshot
    // and sync for all.
    void FixedUpdate() {
        _timeUntilNextUpdate -= Time.fixedDeltaTime;
        if (_timeUntilNextUpdate < 0)
            _timeUntilNextUpdate = c_interval;
        else
            return;

        Game.Instance.UpdateSnapshot();

        // read data from client end points
        foreach (var entry in ep2msg) {
            EndPoint ep = entry.Key;
            Queue<byte[]> msgQueue = entry.Value;

            if (msgQueue.Count == 0) {
                continue; // Skip this client
            }
            // dequeue the latest message from this endpoint
            byte[] currentSnapshotInBytes = msgQueue.Dequeue();

            Snapshot recieved = Snapshot.FromBytes(currentSnapshotInBytes);
            // Debug.Log("player len of " + recieved.playerStates.Length);
            // Debug.Log("cube len of " + recieved.cubeStates.Length);
            // Apply what the client sent us
            Game.Instance.ApplySnapshot(recieved);
        }

        Game.Instance.UpdateSnapshot();

        List<RBObj> priorityPlayers = new List<RBObj>();
        foreach (RBObj player in Game.Instance.Snapshot.playerStates) {
            if(player.IsActive) {
                // Debug.Log(player.Id);
                priorityPlayers.Add(player);
            }
        }
        // priorityPlayers.AddRange(Game.Instance.Snapshot.playerStates);
        List<RBObj> priorityCubes = Game.Instance.Snapshot.getPriorityCubes(120);

        // Now clear the priority value of the cubes we just sent
        // Game.Instance.Snapshot.clearPriority(priorityCubes);

        Snapshot updatedSnapshot = new Snapshot(priorityPlayers, priorityCubes); // Game.Instance.Snapshot;

        // if (lastClientSize != ep2msg.Count) {
        //     updatedSnapshot = Game.Instance.Snapshot; // New player joined, send the whole thing
        // }
        // lastClientSize = ep2msg.Count;

        byte[] updatedSnapshotInBytes = Snapshot.ToBytes(updatedSnapshot);
        // serverSend synced data in bytes to every endpoint

        foreach (var entry in ep2msg) {
            serverSocket.ServerSend(updatedSnapshotInBytes, entry.Key);
        }

    }
}
