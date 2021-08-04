using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; //static instance of the GameManager allows for other classes to have access. 

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>(); //dictionary of players stored on the client side. Using the Player Manager class.  
    public static Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>(); //dictionary of all the itemspawners on the map. 

    public GameObject localPlayerPrefab; //local player prefab
    public GameObject playerPrefab; //prefab for ?????????
    public GameObject itemSpawnerPrefab;

    private void Awake() //before game starts
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation) //used to spawn the player client side using the rotation, postion, username and id and add to dictionary 
    {
        GameObject _player; //local player game object. 
        if (_id == Client.instance.myId) //check that the player we are spawning is the local one. If it is then...
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation); //instantiate the the local player prefab 
        }
        else //if not...
        {
            _player = Instantiate(playerPrefab, _position, _rotation); //instantiate the non local prefab 
        }

        _player.GetComponent<PlayerManager>().Initialize(_id, _username);
        players.Add(_id, _player.GetComponent<PlayerManager>()); //add the player manager script to the player dictionary using the player id as the key. 
    }
    public void CreateItemSpawner(int _spawnerId, Vector3 _position, bool _hasItem) //Used to spawn the item spawners similarly to how players are spawned on the server. 
    {
        GameObject _spawner = Instantiate(itemSpawnerPrefab, _position, itemSpawnerPrefab.transform.rotation);
        _spawner.GetComponent<ItemSpawner>().Initialize(_spawnerId, _hasItem);
        itemSpawners.Add(_spawnerId, _spawner.GetComponent<ItemSpawner>());
    }

}
