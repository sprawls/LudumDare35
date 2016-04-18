using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject[] PossibleWallPrefabs;
    int life = 100;
    float wallSpawnTime = 3f;
    public float wallReachZeroTime = 3f;
    float wallZDistance = 50f;


	// Use this for initialization
	void Start () {
        gameObject.tag = "GameManager";
        StartCoroutine(SpawnWalls());
	}

    IEnumerator SpawnWalls() {
        while (true) {
            yield return new WaitForSeconds(wallSpawnTime);
            SpawnAWall();
        }
    }

    void SpawnAWall() {
        int prefabIndex = Random.Range(0, PossibleWallPrefabs.Length);
        GameObject instantiatedWall = (GameObject)Instantiate(PossibleWallPrefabs[prefabIndex], new Vector3(0, 0, wallZDistance), Quaternion.identity);
    }

}
