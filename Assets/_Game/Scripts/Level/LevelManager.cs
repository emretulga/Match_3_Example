using System.Threading.Tasks;
using UnityEngine;

namespace EmreTulga.MatchThreeExample.Runtime.Core
{
    public class LevelManager : MonoBehaviour
    {
        #region CONSTS-STATICS

        public static readonly Vector2Int[] MATCH_PATTERNS = {new Vector2Int(1, 0), new Vector2Int(0, -1)};

        public const float OBJECT_SLIDING_TIME = 0.02f;
        public const float OBJECT_MAX_SLIDING_TIME = 0.4f;
        public const float DESTROY_OBJECT_TIME = 0.3f;

        public const int WAITING_TIME_TO_CHECK_MOVED_OBJECT_AS_MILLISECONDS = 100;
        public const int MIN_MATCHABLE_OBJECT_AMOUNT = 3;

        #endregion

        #region VARIABLES
        public static LevelManager Instance;
        public LevelData exampleLevel;

        [HideInInspector]
        public Vector2[,]  cellpositions;

        [HideInInspector]
        public MatchObject[,] cellObjects;

        [SerializeField]
        private GameObject _cellGameObject;

        [HideInInspector]
        public int[] nextmatchobjects;

        #endregion

        #region FUNCTIONS
        private void Awake()
        {
            Application.targetFrameRate = 60;

            Instance = this;

            CreateLevel();
        }

        public async void CreateLevel()
        {
            await SetCellsPositions();
            await CreateCells();
            await SetCellObjects();

            SetNextMatchObjects();
        }

        public void CreateNewMatchObject(int columnIndex, int lineIndex, GameObject matchObjectGO)
        {
            Vector2 creatingMatchObjectWorldPosition = new Vector2(cellpositions[columnIndex, lineIndex].x, exampleLevel.MatchObjectCreatingWorldPositionY);

            MatchObject createdMatchObject = Instantiate(matchObjectGO, creatingMatchObjectWorldPosition, Quaternion.identity).GetComponent<MatchObject>();
                    
            createdMatchObject.transform.localScale = Vector3.one * exampleLevel.CellSize;

            createdMatchObject.LandToPosition(new Vector2Int(columnIndex, lineIndex));
        }

        public void ChangeObjectInCell(MatchObject placingObject, Vector2Int targetPosition)
        {
            cellObjects[targetPosition.x, targetPosition.y] = placingObject;
        }

        public void ReleaseObjectInCell(Vector2Int targetPosition)
        {
            cellObjects[targetPosition.x, targetPosition.y] = null;
        }

        public MatchObject GetMatchObjectInCell(Vector2Int targetPosition)
        {
            return cellObjects[targetPosition.x, targetPosition.y];
        }

        public Vector2 GetWorldPositionOfACell(Vector2Int targetPosition)
        {
            return cellpositions[targetPosition.x, targetPosition.y];
        }

        public void CheckFallableObjectsInCells(int columnIndex)
        {
            int cellLineLength = cellObjects.GetLength(1);

            for(int lineIndex = 0; lineIndex < cellLineLength; lineIndex++)
            {
                Vector2Int checkingPosition = new Vector2Int(columnIndex, lineIndex);

                MatchObject objectInCell = GetMatchObjectInCell(checkingPosition);

                bool isObjectExist = objectInCell;

                if(isObjectExist)
                {
                    continue;
                }

                int fallStartingLineIndex = lineIndex;

                if(fallStartingLineIndex >= cellLineLength)
                {
                    break;
                }

                bool isObjectFound = false;

                for(int checkingLineIndex = fallStartingLineIndex; checkingLineIndex < cellLineLength; checkingLineIndex++)
                {
                    Vector2Int checkingFallPosition = new Vector2Int(columnIndex, checkingLineIndex);

                    MatchObject fallingObjectInCell = GetMatchObjectInCell(checkingFallPosition);

                    bool isFallingObjectExist = fallingObjectInCell;

                    if(!isFallingObjectExist || !fallingObjectInCell.IsLanded)
                    {
                        continue;
                    }

                    fallingObjectInCell.Fall();

                    isObjectFound = true;

                    CheckFallableObjectsInCells(columnIndex);

                    break;
                }

                if(isObjectFound)
                {
                    return;
                }

                CreateNextMatchObject(checkingPosition);
            }
        }

        private void SetNextMatchObjects()
        {
            int cellColumnLength = exampleLevel.matchObjectMatrix[0].matchObjectsList.Count;

            nextmatchobjects = new int[cellColumnLength];
        }


        private int GetNextMatchObject(int columnIndex)
        {
            return nextmatchobjects[columnIndex];
        }
        private void ChangeNextMatchObject(int columnIndex, int lineIndex)
        {
            nextmatchobjects[columnIndex] = lineIndex;
        }

        public void CreateNextMatchObject(Vector2Int position)
        {
            int lineIndex = nextmatchobjects[position.x];
            int columnIndex = position.x;
            GameObject matchObjectPrefab = exampleLevel.matchObjectMatrix[lineIndex].matchObjectsList[columnIndex];
            
            CreateNewMatchObject(position.x, position.y, matchObjectPrefab);

            int cellLineLength = exampleLevel.matchObjectMatrix.Count;

            int nextMatchObjectLineIndex = GetNextMatchObject(columnIndex) + 1;

            if(nextMatchObjectLineIndex >= cellLineLength)
            {
                nextMatchObjectLineIndex = 0;
            }

            ChangeNextMatchObject(columnIndex, nextMatchObjectLineIndex);
        }

        #endregion

        #region TASKS
        private async Task SetCellsPositions()
        {
            int cellLineLength = exampleLevel.matchObjectMatrix.Count;
            int cellColumnLength = exampleLevel.matchObjectMatrix[0].matchObjectsList.Count;

            cellpositions = new Vector2[cellColumnLength, cellLineLength];

            for(int lineIndex = 0; lineIndex < cellLineLength; lineIndex++)
            {
                for(int columnIndex = 0; columnIndex < cellColumnLength; columnIndex++)
                {
                    bool isTaskCancelled = !this;
                    if(isTaskCancelled)
                    {
                        return;
                    }

                    Vector2 cellAnchoredPosition = new Vector2(columnIndex, lineIndex) * exampleLevel.CellDistance;
                    Vector2 cellWorldPosition = exampleLevel.CellStartPoint + cellAnchoredPosition;
                    cellpositions[columnIndex, lineIndex] = cellWorldPosition;
                    
                    await Task.Yield();
                }
            }
        }

        private async Task CreateCells()
        {
            int cellLineLength = exampleLevel.matchObjectMatrix.Count;
            int cellColumnLength = exampleLevel.matchObjectMatrix[0].matchObjectsList.Count;

            for(int lineIndex = 0; lineIndex < cellLineLength; lineIndex++)
            {
                for(int columnIndex = 0; columnIndex < cellColumnLength; columnIndex++)
                {
                    bool isTaskCancelled = !this;
                    if(isTaskCancelled)
                    {
                        return;
                    }

                    Transform newCellTransform = Instantiate(_cellGameObject, cellpositions[columnIndex, lineIndex], Quaternion.identity).transform;
                    
                    newCellTransform.localScale = Vector3.one * exampleLevel.CellSize;

                    await Task.Yield();
                }
            }
        }

        private async Task SetCellObjects()
        {
            int cellLineLength = exampleLevel.matchObjectMatrix.Count;
            int cellColumnLength = exampleLevel.matchObjectMatrix[0].matchObjectsList.Count;

            cellObjects = new MatchObject[cellColumnLength, cellLineLength];

            for(int lineIndex = 0; lineIndex < cellLineLength; lineIndex++)
            {
                for(int columnIndex = 0; columnIndex < cellColumnLength; columnIndex++)
                {
                    bool isTaskCancelled = !this;
                    if(isTaskCancelled)
                    {
                        return;
                    }

                    GameObject matchObjectPrefab = exampleLevel.matchObjectMatrix[lineIndex].matchObjectsList[columnIndex];

                    if(matchObjectPrefab == null)
                    {
                        await Task.Yield();

                        continue;
                    }

                    CreateNewMatchObject(columnIndex, lineIndex, matchObjectPrefab);

                    await Task.Yield();
                }
            }
        }

        #endregion
    }
}