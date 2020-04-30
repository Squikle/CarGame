using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    Transform player;

    public GameObject groundPrefab;
    float spawnGroundZ = 475f;
    float groundLenght = 1000f;
    public int groundsOnScreen = 2;
    public float groundHeight = -0.5f;

    public GameObject tilePrefab;
    float spawnZ = 0f;
    float tileLenght = 50f;
    public float safeZone = 300f;
    public int tilesOnScreen = 5;

    List<GameObject> tiles;
    List<GameObject> grounds;
    void Start()
    {
        tiles = new List<GameObject>();
        grounds = new List<GameObject>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i=0; i<tilesOnScreen; i++)
            spawnTile();

        for (int i = 0; i < groundsOnScreen; i++)
            spawnGround();
    }

    void Update()
    {
        if (player.position.z - safeZone > (spawnZ - tilesOnScreen * tileLenght))
        {
            spawnTile();
            deleteTile();
        }
        if (player.position.z - 550f > (spawnGroundZ-groundsOnScreen * groundLenght))
        {
            spawnGround();
            deleteGround();
        }

    }
    private void spawnGround(int prefabIndex = -1)
    {
        GameObject go = Instantiate(groundPrefab) as GameObject;
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(0, groundHeight, spawnGroundZ);
        spawnGroundZ += groundLenght;
        grounds.Add(go);
    }
    private void deleteGround()
    {
        Destroy(grounds[0]);
        grounds.RemoveAt(0);

        Debug.Log("deleted");
    }

    private void spawnTile(int prefabIndex = -1)
    {
        GameObject go = Instantiate(tilePrefab) as GameObject;
        go.transform.SetParent(transform);
        go.transform.position = Vector3.forward * spawnZ;
        spawnZ += tileLenght;
        tiles.Add(go);
    }

    private void deleteTile()
    {
        Destroy(tiles[0]);
        tiles.RemoveAt(0);
    }
}
