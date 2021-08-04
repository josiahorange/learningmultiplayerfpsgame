using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet) //setting up to send the tcp packet. 
    {
        _packet.WriteLength(); //places the number of bytes of message to be sent at the start of the packet.
        Client.instance.tcp.SendData(_packet);  //sending the message packet via the tcp stream using the client id through client.cs
    }

    //UDP//
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength(); //places the number of bytes of message to be sent at the start of the packet.
        Client.instance.udp.SendData(_packet); //sending the message packet via the tcp stream using the client id through client.cs
    }



    #region Packets
    public static void WelcomeReceived() //responding to the welcome message. 
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived)) //(int)ClientPackets.welcome is the id of the packet passed into the new packet instance (1 because its the first message (welcome). Set out by the ClientPackets enum in packets.cs)
        {
            _packet.Write(Client.instance.myId); // writing the clients id into the packet. 
            _packet.Write(UIManager.instance.usernameField.text); //writing the user entered username to the packet response. 

            SendTCPData(_packet); //calling the SendTCPData method to prepare for packet sending through the stream. 
        }
    }


    public static void PlayerMovement(bool[] _inputs) //method for sending the players keyboard inputs to the server. 
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement)) //using the the packet of id from playermovement.
        {
            _packet.Write(_inputs.Length); //write number of inputs. 
            foreach (bool _input in _inputs) //write each input into the packet. 
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation); //write the players rotation to the packet. This rotation is found from the player object through the playermanager script attached to the player. 

            SendUDPData(_packet); //We can use this because we are sending these packets so often and we can use the UDP speed.

        }
    }

    /*public static void PlayerMovement(Vector2 mag, bool grounded, float x, float y, bool jumping, bool crouching, bool sprinting, Vector3 rot, float mouseY, float mouseX) //method for sending the players keyboard inputs to the server. 
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement)) //using the the packet of id from playermovement.
        {
           _packet.Write(mag);
           _packet.Write(rot); 
           _packet.Write(mouseX); 
           _packet.Write(mouseY);
           _packet.Write(mouseY);
           _packet.Write(x);
           _packet.Write(y);
           _packet.Write(jumping);
           _packet.Write(crouching);
           _packet.Write(sprinting);
           _packet.Write(grounded);




           SendUDPData(_packet); //We can use this because we are sending these packets so often and we can use the UDP speed.

        }
    } */

    public static void PlayerShoot(Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            _packet.Write(_facing);
            SendTCPData(_packet);
        }
    }

    #endregion
}
