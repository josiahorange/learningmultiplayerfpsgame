using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{

    #region Packets
    public static void Welcome(Packet _packet) //recieving the Welcome packet.
    {
        string _msg = _packet.ReadString(); //reading the message and id the same way we put them in to make sure it works.
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}"); //display message
        Client.instance.myId = _myId; //storing the client id in the instance. 
        ClientSend.WelcomeReceived();


        //UDP//
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port); //call the connect method and pass in the local port (that the tcp method is using). 
    }

    public static void SpawnPlayer(Packet _packet) //for receiving the packet that contains all the information about the new player.
    {
        int _id = _packet.ReadInt(); 
        string _username = _packet.ReadString(); 
        Vector3 _position = _packet.ReadVector3(); 
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation); //Call the spawn player method to call the player int the game
    }

    /*public static void PlayerPosition(Packet _packet) //handling the players position packet to update yourself and other players positions.
    {
        int _id = _packet.ReadInt(); //get player id
        float _moveSpeed= _packet.ReadFloat();
        float _multiplier = _packet.ReadFloat();
        float _multiplierV = _packet.ReadFloat();
        bool _grounded = _packet.ReadBool();
        bool _jumping = _packet.ReadBool();
        bool _sprinting = _packet.ReadBool();
        bool _crouching = _packet.ReadBool();
        float _x = _packet.ReadFloat();
        float _y = _packet.ReadFloat();
        Vector2 _mag = _packet.ReadVector2();

        Debug.Log(_grounded);

        GameManager.players[_id].clientMovement.setMovement(_grounded, _jumping, _crouching, _sprinting);
        GameManager.players[_id].clientMovement.DoMovement(_moveSpeed, _multiplier, _multiplierV, _x, _y, _mag);//change the position of the players based on their id in the game.
    } */
    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet) //handling other players rotations. 
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rotation; //change the rotation of the players based on their id in the game. 

       
    }

    /*public static void PlayerRotation(Packet _packet) //handling other players rotations. 
    {
        Debug.Log("boy");
        int _id = _packet.ReadInt();
        float _rotation = _packet.ReadFloat();
        float _desired = _packet.ReadFloat();

        GameManager.players[_id].clientMovement.ClientLook(_rotation, _desired);


    } */

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.players[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void CreateItemSpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 _spawnerPosition = _packet.ReadVector3();
        bool _hasItem = _packet.ReadBool();

        GameManager.instance.CreateItemSpawner(_spawnerId, _spawnerPosition, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _byPlayer = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemPickedUp();
        GameManager.players[_byPlayer].itemCount++;
    }

    #endregion
}