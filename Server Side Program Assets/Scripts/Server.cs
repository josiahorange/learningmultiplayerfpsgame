using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Server
{
    public static int MaxPlayers { get; private set; } //max players
    public static int Port { get; private set; }  //the port ?????????????????
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); //a dictionary to track the clients
    public delegate void PacketHandler(int _fromClient, Packet _packet); //delegate of packet handlers which has parameters of both the packet and the client id. 
    public static Dictionary<int, PacketHandler> packetHandlers;


    private static TcpListener tcpListener; //the tcp listener
    private static UdpClient udpListener; //the udp listener managing all udp communication. 

    public static void Start(int _maxPlayers, int _port) //called at start of server
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData(); //adding the clients to client dictionary.

        tcpListener = new TcpListener(IPAddress.Any, Port); //assigning value to tcpListener instance to listen to any ip address at specific port 
        tcpListener.Start(); //Begins Listenting
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null); //calling callback after connection to client. 


        //UDP//
        udpListener = new UdpClient(Port); //instantiating the udp listener. 
        udpListener.BeginReceive(UDPReceiveCallback, null); //begin receiving from the listener. When received call callback method. (below). 

        Debug.Log($"Server started on port {Port}.");
    }


    //Callback after client connection.
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result); //storing connected client as an instance of TcpClient using tcplistener result and temporarily closing the listening. 
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null); //Accepting new client stream starting process again
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        //assigning the incoming client a spot in the dictionary (essentially int the server). 
        for (int i = 1; i <= MaxPlayers; i++) //cycling through all clients in dictionary with a maximum capacity. 
        {
            if (clients[i].tcp.socket == null) //if client slot is empty in dictionary then they can connect there.
            {
                clients[i].tcp.Connect(_client); //handing in new client from tcplistener result into the connect function. 
                return;
            }
        }

        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!"); //if no client slots empty then server is full. 
    }

    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0); //end point with no specific ip address or port. 
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint); //pass the result and the enpoint into the data byte array. This will also set the ip endpoint to the endpoint it came from
            udpListener.BeginReceive(UDPReceiveCallback, null); //begin receiving data again.

            if (_data.Length < 4) // if the data is less than 4 bytes long then cancel
            {
                return;
            }

            using (Packet _packet = new Packet(_data)) //create new packet using the byte data array
            {
                int _clientId = _packet.ReadInt(); //store client id. 

                if (_clientId == 0) //if the client id is 0 then cancel (prevents a server crash while looking for client 0)
                {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null) //if the senders udp endpoint is null meaning this is a new connection (if so it will be the empty one that opens up the port)
                {
                    clients[_clientId].udp.Connect(_clientEndPoint); //call the clients connect method, passing the endpoint without using any of the data (which doesnt exist). 
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString()) //check if the endpoint we have stored for the client is the same as the endpoint. 
                {
                    clients[_clientId].udp.HandleData(_packet); //handle the data passing in the data to the correct client
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving UDP data: {_ex}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null) //make sure the endpoint isnt null before sending
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null); //begin sending to stream using the data passed in 
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    #region Packets
    private static void InitializeServerData()
    {
        //adding clients to dictionary data structure depending on the max players. 
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                {(int)ClientPackets.playerShoot, ServerHandle.PlayerShoot}


            };
        Debug.Log("Initialized packets");
    }
    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
    #endregion
}
