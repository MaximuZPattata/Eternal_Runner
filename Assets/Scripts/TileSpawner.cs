using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace EternalRunner
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject startingTile;

        [SerializeField]
        private List<GameObject> turnTilesList;

        [SerializeField]
        private List<GameObject> obstaclesList;

        [SerializeField]
        private int tileStartCount = 10;

        [SerializeField]
        private int minimumStraightTiles = 5;

        [SerializeField]
        private int maximumStraightTiles = 20;

        private Vector3 startingTileOffset = new Vector3(0.0f, -0.075f, 4.8f);

        private Vector3 nextTileLocation = Vector3.zero;
        private Vector3 nextTileDirection = Vector3.forward;
        private GameObject previousTile;

        private List<GameObject> tilesUsedCurrentlyList;
        private List<GameObject> obstaclesUsedCurrentlyList;

        private void Start()
        {
            tilesUsedCurrentlyList = new List<GameObject>();
            obstaclesUsedCurrentlyList = new List<GameObject>();

            Random.InitState(System.DateTime.Now.Millisecond);

            if (nextTileLocation == Vector3.zero)
                nextTileLocation += startingTileOffset;

            for (int i = 0; i < tileStartCount; i++)
                SpawnNewTile(startingTile.GetComponent<Tile>());

            SpawnNewTile(SelectRandomTurnTileFromList(turnTilesList).GetComponent<Tile>());
        }

        private void SpawnNewTile(Tile tileToBeSpawned, bool spawnObstacle = false)
        {
            Quaternion newTileRotation = tileToBeSpawned.gameObject.transform.rotation * Quaternion.LookRotation(nextTileDirection, Vector3.up);

            previousTile = GameObject.Instantiate(tileToBeSpawned.gameObject, nextTileLocation, newTileRotation);

            tilesUsedCurrentlyList.Add(previousTile);

            nextTileLocation += Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size, nextTileDirection);
        }

        private GameObject SelectRandomTurnTileFromList(List<GameObject> turnTileList)
        {
            if (turnTileList.Count == 0)
                return null;

            return turnTileList[Random.Range(0, turnTileList.Count)];
        }
    }
}
