using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ServerHandle
{
    #region Packets
    public static void WelcomeReceived(int _fromClient, Packet _packet) // receiving the client id (who send it) and the packet itself. 
    {
        int _clientIdCheck = _packet.ReadInt(); //reading the message and id the same way we put them in to make sure it works.
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}."); //displaying to the console that the player connected successfully. 
        if (_fromClient != _clientIdCheck)  //if the client id is not from the right client. 
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    /*public static void PlayerMovement(int _fromClient, Packet _packet) //method for handling the player position and rotation package. 
    {

        Vector2 mag = _packet.ReadVector2();
        Vector3 rot = _packet.ReadVector3(); //reading all the booleans from the packet. 
        float mouseX = _packet.ReadFloat();
        float mouseY = _packet.ReadFloat();
        float x = _packet.ReadFloat();
        float y = _packet.ReadFloat();
        bool jumping = _packet.ReadBool();
        bool crouching = _packet.ReadBool();
        bool sprinting = _packet.ReadBool();
        bool grounded = _packet.ReadBool();


        Server.clients[_fromClient].player.UpdateLook(rot, mouseX, mouseY); //calling the setinput method to store these received values in the player instance. 
        Server.clients[_fromClient].player.UpdateMovement(mag, grounded, x, y, jumping, crouching, sprinting); //calling the setinput method to store these received values in the player instance. 
     
    } */

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.Shoot(_shootDirection);
    }
    #endregion

}
