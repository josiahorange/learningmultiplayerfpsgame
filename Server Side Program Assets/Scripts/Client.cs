using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public static int dataBufferSize = 4096; //reserved buffer storage ????????

    public int id; //the clients id. 
    public TCP tcp; //the clients tcp instance (see below)
    public UDP udp;
    public Player player;

    public Client(int _clientId) //constructure
    {
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket; //tcp client called socket (recieved from server.cs)  A CLIENT IS A SOCKET

        private readonly int id; //id ???
        private NetworkStream stream; //new stream for sending and recieving data through sockets.
        private Packet receivedData;
        private byte[] receiveBuffer; //array of bytes as storage buffer for recieving. 

        public TCP(int _id) //constructure
        {
            id = _id;
        }

        public void Connect(TcpClient _socket) //connecting client to spot in the server.
        {
            socket = _socket; //the client information from server.cs
            socket.ReceiveBufferSize = dataBufferSize; //assigning buffer size to client while recieving. (the size of the receiving packet - needs to be the same at client and server). 
            socket.SendBufferSize = dataBufferSize; //assigning buffer size to client while sending. (the size of the receiving packet - needs to be the same at client and server).

            stream = socket.GetStream(); //get the network stream from the client instance.

            receivedData = new Packet();

            receiveBuffer = new byte[dataBufferSize]; //instanciating the recievebuffer with the size of the buffer chosen at the start. 

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null); //reading from the data stream using the data buffer size and recieve buffer array and calling back to the method once data has bveen recieved. 

            ServerSend.Welcome(id, "Welcome to the server!");
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null); //writing to the stream using the packet and all its information. 
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to player {id} via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result) //taking in the stream recieve result. 
        {
            try
            {
                int _byteLength = stream.EndRead(_result); //stoping read temporarily.
                if (_byteLength <= 0) //if the read was empty then disconnect
                {
                    Server.clients[id].Disconnect(); //calls all the disconnecting from tcp and udp etc. 
                    return;
                }

                byte[] _data = new byte[_byteLength]; //array to hold data recieved with length of the recieved data
                Array.Copy(receiveBuffer, _data, _byteLength); //data copied to _data

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null); //start up recieving stream again.
            }
            catch (Exception _ex) //if there is error then disconnect.
            {
                Debug.Log($"Error receiving TCP data: {_ex}");
                Server.clients[id].Disconnect(); //calls all the disconnecting from tcp and udp etc. 
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;
            receivedData.SetBytes(_data); //receiveData is assigned the received packets from the stream

            if (receivedData.UnreadLength() >= 4) //if the data contains more than 4 unread bytes (an integer representing the length of the packet) and that length is less than 0 then return true
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }



            //if the length of the packet is more than 0 then.
            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) //while this is running the data contains another complete packet in which we can handle.
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength); //reading the received data into an array of bytes. 

                ThreadManager.ExecuteOnMainThread(() => //done on one thread
                {
                    using (Packet _packet = new Packet(_packetBytes)) //using the data...
                    {
                        int _packetId = _packet.ReadInt(); //finding the packet id. 
                        Server.packetHandlers[_packetId](id, _packet); //grabbing the appropriate packet handler using the packet id to invoke the handler passing in the packet data. 
                    }
                });

                _packetLength = 0;

                if (receivedData.UnreadLength() >= 4) //if the data contains more than 4 unread bytes (an integer representing the length of the packet) and that length is less than 0 then return true
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;

        }

        public void Disconnect() //for disconnecting the client and closing all the parts. 
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint; //a client called socket. SAME THING. 

        private int id; //clients id 

        public UDP(int _id) //initialising the id
        {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint) //connecting to the client
        {
            endPoint = _endPoint;
        }

        public void SendData(Packet _packet) //sending via udp. 
        {
            Server.SendUDPData(endPoint, _packet); //passing endpoint info and the packet to the method to send the packet.
        }

        public void HandleData(Packet _packetData) //passing in data packet to be handled. 
        {
            int _packetLength = _packetData.ReadInt(); //store packet length
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength); //store the number of bytes specificed by the packet length into a byte array.

            ThreadManager.ExecuteOnMainThread(() => //execute on main thread
            {
                using (Packet _packet = new Packet(_packetBytes)) //create a new packet using the byte array.
                {
                    int _packetId = _packet.ReadInt(); //store packet id 
                    Server.packetHandlers[_packetId](id, _packet); //invoke the appropriate packet handler method from the dictionary using the id. 
                }
            });
        }


        public void Disconnect() //disconnecting the client from the udp connection. 
        {
            endPoint = null; //clearning the endpoint
        }
    }

    public void SendIntoGame(string _playerName) //passing player name
    {
        //creating a new player after sendintogame is called
        player = NetworkManager.instance.InstantiatePlayer(); //creating new player instance with all the right information (including the player name, id (which is the same as the client id) and its position) This will be using a game object.
        player.Initialize(id, _playerName);
        //for each client in the client dictionary that exists, send the information about the these players to the new player. 
        foreach (Client _client in Server.clients.Values)
        {

            if (_client.player != null) //if this client has 
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }

        //for each client in the client dictionary that exists, send the information about the new player to everyone else that exists. 
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }

        foreach (ItemSpawner _itemSpawner in ItemSpawner.spawners.Values) //sending informaton about the item spawners to spawn them in client side once the player is spawned
        {
            ServerSend.CreateItemSpawner(id, _itemSpawner.spawnerId, _itemSpawner.transform.position, _itemSpawner.hasItem);
        }

    }
    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected."); //telling the console that the player has disconnected


        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect(); //calling the disconnect methods (above)
        udp.Disconnect();

        ServerSend.PlayerDisconnected(id);
    }
}
