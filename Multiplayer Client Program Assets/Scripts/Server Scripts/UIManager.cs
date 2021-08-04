using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{   //making an instance of the UIManager class and checking it doesnt already exist on the awake of the game.
    public static UIManager instance; 

    public GameObject startMenu; //the start menu object (chosen in engine)
    public InputField usernameField; //the username field (chosen in engine)

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this){
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ConnectToServer() //called after the connect button is pressed in the start menu. 
    {
        startMenu.SetActive(false); //turn off start menu once connecting. 
        usernameField.interactable = false; //can no longer change the username field. 
        Client.instance.ConnectedToServer(); // call the connect to server method. 
    }
}
