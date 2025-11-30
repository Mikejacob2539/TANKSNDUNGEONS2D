using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct ItemAndChance
{
    public GameObject item;
    public int weight;
}
public class Item_Spawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] ItemAndChance[] items;
    int totalWeight = 0;

    GameObject itemToSpawn;
    [SerializeField] LayerMask groundLayer;
    PlayerMovement move;
    int spawnAmounts = 3;


    void Start()
    {
        move = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        CalculateTotalWeight();
        EnemyController.item += NowWEaCTUALLYsPAWN;
        Enemy_Turret_Controler.item += NowWEaCTUALLYsPAWN;
    }



    void CalculateTotalWeight()
    {
        foreach (var item in items)
            totalWeight += item.weight;
    }

    GameObject SpawnLogic()
    {
        int randomInt = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;
        foreach (var item in items)
        {
            cumulativeWeight += item.weight;
            if (randomInt < cumulativeWeight)
                return item.item;
            else
                continue;

        }

        return null;
    }


    void NowWEaCTUALLYsPAWN(Vector2 pos)
    {
        for (int i = 0; i < spawnAmounts; i++)
        {
            itemToSpawn = SpawnLogic();
            var orb = Instantiate(itemToSpawn, pos, Quaternion.identity).GetComponent<OrbsMagnet>();
            orb.ScatterForce = Random.Range(1.1F, 1.6F);

        }

    }

}
