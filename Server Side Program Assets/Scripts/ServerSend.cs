using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    private static void SendTCPData(int _toClient, Packet _packet) //passing the client and dictionary id and the packet itself to be sent (currently contains message and client id)
    {
        _packet.WriteLength(); //places the number of bytes of message to be sent at the start of the packet.
        Server.clients[_toClient].tcp.SendData(_packet); //sending the message packet via the tcp stream using the client id through client.cs
    }


    private static void SendTCPDataToAll(Packet _packet) //this sends data message packet to all clients
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet) //this sends data message packet to all clients but one
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    public static void Welcome(int _toClient, string _msg) //passing client id and message from client.cs
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome)) //(int)ServerPackets.welcome is the id of the packet passed into the new packet instance (1 because its the first message (welcome). Set out by the ServerPackets enum in packets.cs)
        {
            _packet.Write(_msg); //writes the message to the packet
            _packet.Write(_toClient); //writes the client dictionary id to the packet.

            SendTCPData(_toClient, _packet); //passing id and packet instance.
        }
    }

    //Method for sending player his username, id, position and rotation. IMPORTANT PACKAGE for spawning the player. 
    public static void SpawnPlayer(int _toClient, Player _player) //passing client/player id and the player instance. 
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer)) //creating new packet instance using the spawn player id. 
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }


    public static void PlayerPosition(Player _player) //method for sending all clients the player positions. 
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition)) //new packet instance with appropriate id. 
        {
            _packet.Write(_player.id); //write the players position and id to the packet
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet); //send via udp. 
        }
    } 

    public static void PlayerRotation(Player _player)  //method for sending all clients but the player the player rotation. 
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet); //send via udp.
        }
    }


    /*public static void PlayerPosition(Player _player, float moveSpeed, float multiplier, float multiplierV, bool grounded, bool jumping, bool sprinting, bool crouching, float x, float y, Vector2 mag) //method for sending all clients the player positions. 
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition)) //new packet instance with appropriate id. 
        {
            _packet.Write(_player.id); //write the players position and id to the packet
            _packet.Write(moveSpeed);
            _packet.Write(multiplier);
            _packet.Write(multiplierV);
            _packet.Write(grounded);
            _packet.Write(jumping);
            _packet.Write(sprinting);
            _packet.Write(crouching);
            _packet.Write(x);
            _packet.Write(y);
            _packet.Write(mag);




            SendUDPDataToAll(_packet); //send via udp. 
        }
    } */

    /*public static void PlayerRotation(Player _player, float xRotation, float desiredX )  //method for sending all clients but the player the player rotation. 
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(xRotation);
            _packet.Write(desiredX);


            SendUDPDataToAll(_player.id, _packet); //send via udp.
        }
    }*/

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerHealth(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerRespawned(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }

    public static void CreateItemSpawner(int _toClient, int _spawnerId, Vector3 _spawnerPosition, bool _hasItem) //creating the item spawner packet to send the items position id and player boolean
    {
        using (Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            _packet.Write(_spawnerId); 
            _packet.Write(_spawnerPosition);
            _packet.Write(_hasItem);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ItemSpawned(int _spawnerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemSpawned))
        {
            _packet.Write(_spawnerId);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ItemPickedUp(int _spawnerId, int _byPlayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_byPlayer);

            SendTCPDataToAll(_packet);
        }
    }


    #endregion
}
