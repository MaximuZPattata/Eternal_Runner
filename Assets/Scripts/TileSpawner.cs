using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace EternalRunner
{
    public class TileSpawner : MonoBehaviour
    {
        #region Defining variables

        [SerializeField]
        private GameObject startingTile;

        [SerializeField]
        private List<GameObject> allTurnTilesList;

        [SerializeField]
        private List<GameObject> allObstaclesList;

        [SerializeField]
        private int tileStartCount = 10;

        [SerializeField]
        private int minimumStraightTiles = 5;

        [SerializeField]
        private int maximumStraightTiles = 20;

        private Vector3 startingTileOffset = new Vector3(0.0f, -0.075f, 4.8f);

        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject previousTile;

        private List<GameObject> tilesUsedCurrentlyList;
        private List<GameObject> obstaclesUsedCurrentlyList;

        #endregion

        #region Private functions
        private void Start()
        {
            tilesUsedCurrentlyList = new List<GameObject>();
            obstaclesUsedCurrentlyList = new List<GameObject>();

            Random.InitState(System.DateTime.Now.Millisecond);

            if (currentTileLocation == Vector3.zero)
                currentTileLocation += startingTileOffset;

            for (int i = 0; i < tileStartCount; i++)
                SpawnNewTile(startingTile.GetComponent<Tile>());

            SpawnNewTile(SelectRandomGameObjectFromList(allTurnTilesList).GetComponent<Tile>());
        }

        private void SpawnNewTile(Tile tileToBeSpawned, bool spawnObstacle = false)
        {
            Quaternion newTileRotation = tileToBeSpawned.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

            previousTile = GameObject.Instantiate(tileToBeSpawned.gameObject, currentTileLocation, newTileRotation);

            tilesUsedCurrentlyList.Add(previousTile);

            if (spawnObstacle)
                SpawnObstacles();

            if(previousTile.GetComponent<Tile>().type == TileType.STRAIGHT)
                currentTileLocation += Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
        }

        private void SpawnObstacles()
        {
            if (Random.value > 0.4f) 
                return;

            GameObject obstacleGameObject = SelectRandomGameObjectFromList(allObstaclesList);

            Quaternion newObstacleRotation = obstacleGameObject.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

            GameObject newObstacle = Instantiate(obstacleGameObject, currentTileLocation, newObstacleRotation);

            obstaclesUsedCurrentlyList.Add(newObstacle);
        }

        private void DeletePreviousTiles()
        {
            while(tilesUsedCurrentlyList.Count != 1)
            {
                GameObject tileToBeDeleted = tilesUsedCurrentlyList[0];
                
                tilesUsedCurrentlyList.RemoveAt(0);
                Destroy(tileToBeDeleted);
            }

            while(obstaclesUsedCurrentlyList.Count != 0)
            {
                GameObject obstaclesToBeDeleted = obstaclesUsedCurrentlyList[0];

                obstaclesUsedCurrentlyList.RemoveAt(0);
                Destroy(obstaclesToBeDeleted);
            }
        }

        private GameObject SelectRandomGameObjectFromList(List<GameObject> gameObjectsList)
        {
            if (gameObjectsList.Count == 0)
                return null;

            return gameObjectsList[Random.Range(0, gameObjectsList.Count)];
        }

        #endregion

        #region Public functions
        public void UpdateNewTileDirection(Vector3 direction)
        {
            currentTileDirection = direction;
            DeletePreviousTiles();

            Vector3 nextTileLocationOffset;

            if (previousTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
            {
                nextTileLocationOffset = Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size / 2
                    + (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), currentTileDirection);
            }
            else // Left/Right Turn Tiles
            {
                nextTileLocationOffset = Vector3.Scale((previousTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2))
                    + (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), currentTileDirection);
            }

            currentTileLocation += nextTileLocationOffset;

            int totalPathLength = Random.Range(minimumStraightTiles, maximumStraightTiles);

            for (int i = 0; i < totalPathLength; i++)
                SpawnNewTile(startingTile.GetComponent<Tile>(), (i == 0) ? false : true);

            SpawnNewTile(SelectRandomGameObjectFromList(allTurnTilesList).GetComponent<Tile>());
        }

        #endregion
    }
}
