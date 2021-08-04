using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance; //instance of the client class.
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1"; //ip of current pc (local host). 

    public int port = 33623;
    public int myId = 0; //id ???????
    public TCP tcp; //create tcp in memory.     
    public UDP udp; //create udp in memory

    private bool isConnected = false;
    private delegate void PacketHandler(Packet packet); //delegate that passes in packet used to reference a handling method. 
    private static Dictionary<int, PacketHandler> packetHandlers; //dictionary of the delegate packet handlers for each packet ascosiated with each packet id. 

    private static string externalip = new WebClient().DownloadString("http://icanhazip.com");

    private void Awake() //before game starts
    {
        if (instance == null) //if instance allready exists then kill it. 
        {
            instance = this;

        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //on game start
    private void Start()
    {
        tcp = new TCP(); //create new tcp instance. 
        udp = new UDP(); //create new udp instance. 
    }


    private void OnApplicationQuit()
    {
        Disconnect(); //call the disconnect method when the application closes. 
    }

    public void ConnectedToServer()
    {
        InitializeClientData();

        isConnected = true;
        tcp.Connect(); //calling the tcp connection

    }

    public class TCP
    {
        public TcpClient socket; //the client itself.  A CLIENT IS A SOCKET
        private NetworkStream stream; //the data stream.
        private Packet receivedData; //the recieved data from ????????????????
        private byte[] receiveBuffer; //buffer for recieving. 



        public void Connect() //on connect
        {
            socket = new TcpClient //creating instance of client and assigning its buffer sizes. 
            {
                ReceiveBufferSize = dataBufferSize, //assigning buffer size to client while recieving. (the size of the receiving packet - needs to be the same at client and server). 
                SendBufferSize = dataBufferSize //assigning buffer size to client while sending. (the size of the receiving packet - needs to be the same at client and server).
            };

            receiveBuffer = new byte[dataBufferSize]; //instanciating the recievebuffer with the size of the buffer chosen at the start. 

            string ipadd = externalip;
            if (externalip == ipadd)
            {
                ipadd = "127.0.0.1";
            }
            else
            {
                Debug.Log("poo");
                ipadd = instance.ip;
            }
            socket.BeginConnect(ipadd, instance.port, ConnectCallback, socket); //start client connect using ip and port and the client and calls a callback once connected to server.

        }

        private void ConnectCallback(IAsyncResult _result) //called once connected. 
        {
            socket.EndConnect(_result); //close connection temporarily.

            if (!socket.Connected) //if not connected then end. 
            {
                return;
            }
            //if connected then...
            stream = socket.GetStream(); //get the data stream from the client socket 

            receivedData = new Packet(); //initialised the recieved data packet. 

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null); //reading from the data stream using the data buffer size and recieve buffer array and calling back to the method once data has bveen recieved.
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null) //if the clients socket doesnt already have a value assigned. 
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null /*(no callback)*/, null); //writing to the stream using the packet and all its information. 
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result) //taking in the stream recieve result. 
        {
            try
            {
                int _byteLength = stream.EndRead(_result); //stoping read temporarily.
                if (_byteLength <= 0) //if the read was empty then disconnect
                {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength]; //array to hold data recieved of length of the received data
                Array.Copy(receiveBuffer, _data, _byteLength); //data copied to _data

                receivedData.Reset(HandleData(_data)); //if the data doesnt have any unmatched bytes then you can reset the data.
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null); //start up recieving stream again.
            }
            catch (Exception _ex) //if there is error then disconnect.
            {
                Disconnect();
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
                        packetHandlers[_packetId](_packet); //grabbing the appropriate packet handler using the packet id to invoke the handler passing in the packet data. 
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

        private void Disconnect() //disconnecting from the tcp instance
        {
            instance.Disconnect(); //main cient class disconnect method called for instance (below).

            stream = null; //ending stream
            receivedData = null; //emptying all the variables.
            receiveBuffer = null;
            socket = null;
        }

    } //TCP OPTION


    public class UDP //UDP OPTION
    {
        public UdpClient socket; //a client called socket. SAME THING. 
        public IPEndPoint endPoint; //the end point ip. 

        public UDP()
        {

            string ipadd = externalip;
            if (externalip == ipadd)
            {
                ipadd = "127.0.0.1";

            }
            else
            {
                ipadd = instance.ip;
            }

            endPoint = new IPEndPoint(IPAddress.Parse(ipadd), instance.port); //new instance of the ip endpoint containing the port and ip. 

        }

        public void Connect(int _localPort) //this port is the port in which the client is communicating. (different from the server port number). 
        {
            socket = new UdpClient(_localPort); //instantiate the client with the server port. 

            socket.Connect(endPoint); //calling the connect method of the client using the desired endpoint. 
            socket.BeginReceive(ReceiveCallback, null); //begin receiving from the server. (as well as passing in the receive callback method. 

            using (Packet _packet = new Packet()) //this sends an empty packet to initiate the connection to the server and open up the local port. 
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId); //insert the client id into the packet. (the server uses this to determine who sent it). This is because of the way UDP works to prevent port closure etc
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null); //begin sending using the packets bytes and the number of bytes.

                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result) //callback once received data. 
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint); //creating a byte array to store the data. Assigning the value of the receive. 
                socket.BeginReceive(ReceiveCallback, null); //start receiving again 

                //THIS CODE MIGHT CAUSE PROBLEMS BECAUSE OF PACKET SPLITTING AND THE WAY UDP WORKS (AT HIGH TRAFFIC). 

                if (_data.Length < 4) //make sure the array has at least 4 bytes (the integer (length) at the beggining). 
                {
                    instance.Disconnect(); //client instance disconnect method
                    return;
                }

                HandleData(_data); //calling the handle data method with the data received. 
            }
            catch
            {
                Disconnect(); //udp disconnect method
            }
        }

        private void HandleData(byte[] _data)
        {

            //removing the first 4 bytes from byte the array (the length of the packet). 
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt(); //storing the packet length. 
                _data = _packet.ReadBytes(_packetLength); //reading this length (specified amount of bytes) into the data variable. 
            }

            ThreadManager.ExecuteOnMainThread(() => //executing on main thread.
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt(); //fetching the packet id. 
                    packetHandlers[_packetId](_packet); //calling the packethandler method appropriate to this packet. (taking it from the dictionary using the packet id).
                }
            });
        }

        private void Disconnect() //udp disconnect
        {
            instance.Disconnect(); //main cient class disconnect method called for instance (below).

            endPoint = null; //emptying variables
            socket = null;
        }
    }

    #region Packets
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>() //instantiating the packethandler dictionary. 
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome}, //first packet with packet id 1 ((int)ServerPackets.welcome) whith the handler ClientHandle.Welcome
            {(int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
            {(int)ServerPackets.playerPosition, ClientHandle.PlayerPosition},
            {(int)ServerPackets.playerRotation, ClientHandle.PlayerRotation},
            {(int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected},
            {(int)ServerPackets.playerHealth, ClientHandle.PlayerHealth},
            {(int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned},
            {(int)ServerPackets.createItemSpawner, ClientHandle.CreateItemSpawner},
            {(int)ServerPackets.itemSpawned, ClientHandle.ItemSpawned},
            {(int)ServerPackets.itemPickedUp, ClientHandle.ItemPickedUp},
            

        };
        Debug.Log("Initialized packets");
    }
    #endregion

    private void Disconnect() 
    {
        if (isConnected) //if the player is connected
        {
            isConnected = false; //turn off bool
            tcp.socket.Close(); //close tcp socket
            udp.socket.Close(); //close the udp socket. 

            Debug.Log("Disconnected from server."); 
        }
    }

}
