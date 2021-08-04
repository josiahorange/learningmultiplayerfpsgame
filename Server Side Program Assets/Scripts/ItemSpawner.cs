using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static Dictionary<int, ItemSpawner> spawners = new Dictionary<int, ItemSpawner>(); //keeping record of all item spawners in the scene. 
    private static int nextSpawnerId = 1; //id of next spawner to be created. 

    public int spawnerId;
    public bool hasItem = false;

    private void Start()
    {
        hasItem = false;
        spawnerId = nextSpawnerId; //spawnid to the next available spawn id. 
        nextSpawnerId++; //increment spawn ids. 
        spawners.Add(spawnerId, this); //add the instance to the dictionary

        StartCoroutine(SpawnItem()); //start the item spawn coroutine.
    }

    private void OnTriggerEnter(Collider other) //after a collision
    {
        if (hasItem && other.CompareTag("Player")) //if the triggered collideer is player then 
        { 
            Player _player = other.GetComponent<Player>();
            if (_player.AttemptPickupItem())
            {
                ItemPickedUp(_player.id); //picked up. 
            }
        }
    }

    private IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(10f); //spawns after 10 seconds. 

        hasItem = true;
        ServerSend.ItemSpawned(spawnerId); //send packet to tell client to spawn the item
    }

    private void ItemPickedUp(int _byPlayer) //when item picked up. 
    {
        hasItem = false; //no longer has item. 
        ServerSend.ItemPickedUp(spawnerId, _byPlayer);

        StartCoroutine(SpawnItem()); //spawn the item again. 
    }
}