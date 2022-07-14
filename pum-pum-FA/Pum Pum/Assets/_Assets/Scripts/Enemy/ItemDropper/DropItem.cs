using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;

public class DropItem : MonoBehaviour
{
    [SerializeField]
    private List<ItemSpawnData> itemsToDrop = new List<ItemSpawnData>();
    private float[] itemWeights;

    [SerializeField]
    [Range(0,1)]
    private float dropChance = 0.5f;

    private void Start()
    {
        itemWeights = itemsToDrop.Select(item => item.rate).ToArray();
    }
    
    public void DropItemData()
    {
        var dropVariable = Random.value;
        if (dropVariable < dropChance)
        {
            int index = GetRandomWeightedIndex(itemWeights);
            Instantiate(itemsToDrop[index].itemPrefab, transform.position, quaternion.identity);
            //Aca hay que hacer el PhotonNetwork.Instantiate(Carpeta de resources con la lista, transform.position, quaternion.identity);
        }
    }

    private int GetRandomWeightedIndex(float[] itemWeights)
    {
        float sum = 0f;
        for (int i = 0; i < itemWeights.Length; i++)
        {
            sum += itemWeights[i];
        }

        float randomValue = Random.Range(0, sum);
        float tempSum = 0;
        for (int i = 0; i < itemsToDrop.Count; i++)
        {
            //0 -> Weight[0] item 1 (0->.05)
            //Weight[0] -> Weight[0] + Weight[i](0.5 -> 0.7
            //tempSun -> tempSu + Weight[N]
            if(randomValue >= tempSum && randomValue < tempSum + itemWeights[i])
            {
                return i;
            }
            tempSum += itemWeights[i];
        }
        return 0;
    }
}

[Serializable]
public struct ItemSpawnData
{
    [Range(0,1)]
    public float rate;
    public GameObject itemPrefab;
}
