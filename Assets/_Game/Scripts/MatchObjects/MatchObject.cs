using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace EmreTulga.MatchThreeExample.Runtime.Core
{
    public class MatchObject : MonoBehaviour
    {
        #region VARIABLES

        public int id;
        
        [HideInInspector]
        public bool IsLanded = false;

        [HideInInspector]
        public Vector2Int CurrentPosition = new Vector2Int(-1, -1);

        [HideInInspector]
        public bool CanInput = false;

        #endregion

        #region FUNCTIONS
        private void OnMouseDown()
        {
            if(!IsLanded)
            {
                return;
            }
            
            PlayerController.Instance.SelectObject(this);
        }

        public async void LandToPosition(Vector2Int targetPosition, bool isUserInput = false)
        {
            if(!IsPositionInRange(targetPosition) || (!CanInput && isUserInput))
            {
                return;
            }

            MatchObject landedOn = null;
            bool IsLandedOnAnObject = false;

            if(IsLanded)
            {
                landedOn = LevelManager.Instance.GetMatchObjectInCell(targetPosition);

                IsLandedOnAnObject = landedOn;

                if(IsLandedOnAnObject)
                {
                    if(!landedOn.IsLanded || !landedOn.CanInput)
                    {
                        return;
                    }
                }
                
                IsLanded = false;

                LevelManager.Instance.ReleaseObjectInCell(CurrentPosition);

                if(IsLandedOnAnObject)
                {
                    landedOn.LandToPosition(CurrentPosition);
                }
            }

            LevelManager.Instance.ChangeObjectInCell(this, targetPosition);

            CanInput = false;

            Vector2Int oldPosition = new Vector2Int(CurrentPosition.x, CurrentPosition.y);

            CurrentPosition = targetPosition;

            await LandingPosition(targetPosition);

            IsLanded = true;

            if(IsMatchingWithAnything())
            {
                DestroyObject();
            }
            else if(isUserInput)
            {
                if(IsLandedOnAnObject)
                {
                    if(!await WaitForCheckingMatch(landedOn))
                    {
                        LandToPosition(oldPosition);
                    }
                    else
                    {
                        CanInput = true;
                    }
                }
                else
                {
                    LandToPosition(oldPosition);
                }
            }
            else
            {
                CanInput = true;
            }
        }
        public void Fall()
        {
            int cellLineLength = LevelManager.Instance.cellObjects.GetLength(1);
            
            for(int lineIndex = 0; lineIndex < cellLineLength; lineIndex++)
            {
                Vector2Int checkingPosition = new Vector2Int(CurrentPosition.x, lineIndex);

                MatchObject objectInCell = LevelManager.Instance.GetMatchObjectInCell(checkingPosition);

                bool isObjectExist = objectInCell;

                if(isObjectExist)
                {
                    continue;
                }

                LandToPosition(checkingPosition);
                break;
            }
        }

        public async void DestroyObject()
        {
            LevelManager.Instance.ReleaseObjectInCell(CurrentPosition);

            IsLanded = false;

            CanInput = false;

            await Destroying();

            LevelManager.Instance.CheckFallableObjectsInCells(CurrentPosition.x);
        }

        private bool IsPositionInRange(Vector2Int targetPosition)
        {
            bool isTargetPositionXInRange = targetPosition.x > -1 && targetPosition.x < LevelManager.Instance.cellpositions.GetLength(0);

            bool isTargetPositionYInRange = targetPosition.y > -1 && targetPosition.y < LevelManager.Instance.cellpositions.GetLength(1);

            bool isTargetPositionInRange = isTargetPositionXInRange && isTargetPositionYInRange;

            return isTargetPositionInRange;
        }

        private bool IsMatchingWithAnything()
        {
            int matchPatternLength = LevelManager.MATCH_PATTERNS.Length;
            
            List<MatchObject>[] matchingObjects = new List<MatchObject>[matchPatternLength];

            int[] matchAmountsOnMatchPatterns = new int[matchPatternLength];

            Vector2Int[] matchPatterns = LevelManager.MATCH_PATTERNS;

            for(int matchPatternIndex = 0; matchPatternIndex < matchPatternLength; matchPatternIndex++)
            {
                matchingObjects[matchPatternIndex] = new List<MatchObject>();

                matchAmountsOnMatchPatterns[matchPatternIndex] = 1;

                Vector2Int patternPosition = CurrentPosition;

                Vector2Int patternOppositePosition = CurrentPosition;

                while(IsPositionInRange(patternPosition))
                {
                    patternPosition += matchPatterns[matchPatternIndex];

                    if(!IsPositionInRange(patternPosition))
                    {
                        break;
                    }

                    MatchObject nextObject = LevelManager.Instance.GetMatchObjectInCell(patternPosition);

                    bool isNextObjectExist = nextObject;

                    if(!isNextObjectExist || !IsMatchingWith(nextObject))
                    {
                        break;
                    }

                    matchingObjects[matchPatternIndex].Add(nextObject);

                    matchAmountsOnMatchPatterns[matchPatternIndex]++;
                }

                while(IsPositionInRange(patternOppositePosition))
                {
                    patternOppositePosition -= matchPatterns[matchPatternIndex];

                    if(!IsPositionInRange(patternOppositePosition))
                    {
                        break;
                    }

                    MatchObject nextObject = LevelManager.Instance.GetMatchObjectInCell(patternOppositePosition);

                    bool isNextObjectExist = nextObject;

                    if(!isNextObjectExist || !IsMatchingWith(nextObject))
                    {
                        break;
                    }

                    matchingObjects[matchPatternIndex].Add(nextObject);

                    matchAmountsOnMatchPatterns[matchPatternIndex]++;
                }
            }
            
            bool IsMatchingWithAnything = false;

            for(int matchPatternIndex = 0; matchPatternIndex < matchPatternLength; matchPatternIndex++)
            {
                if(matchAmountsOnMatchPatterns[matchPatternIndex] < LevelManager.MIN_MATCHABLE_OBJECT_AMOUNT)
                {
                    continue;
                }

                IsMatchingWithAnything = true;

                int matchLength = matchingObjects[matchPatternIndex].Count;

                for(int objectIndex = 0; objectIndex < matchLength; objectIndex++)
                {
                    matchingObjects[matchPatternIndex][objectIndex].DestroyObject();
                }
            }

            return IsMatchingWithAnything;
        }

        private bool IsMatchingWith(MatchObject matchObject)
        {
            bool isObjectIDSame = id == matchObject.id;

            return isObjectIDSame;
        }

        #endregion

        #region TASKS

        public async Task LandingPosition(Vector2Int targetPosition)
        {
            float distanceToTargetPosition = Vector2.Distance(transform.position, targetPosition);

            Vector2 worldPos = LevelManager.Instance.GetWorldPositionOfACell(targetPosition);

            float moveSpeed =  LevelManager.OBJECT_MAX_SLIDING_TIME - distanceToTargetPosition * LevelManager.OBJECT_SLIDING_TIME;

            await transform.DOMove(worldPos, moveSpeed).AsyncWaitForCompletion();
        }

        public async Task Destroying()
        {
            await transform.DOScale(Vector2.zero, LevelManager.DESTROY_OBJECT_TIME).AsyncWaitForCompletion();

            bool isTaskCancelled = this;

            if(!isTaskCancelled)
            {
                return;
            }

            Destroy(this.gameObject);
        }

        public async Task<bool> WaitForCheckingMatch(MatchObject withObject)
        {
            await Task.Delay(LevelManager.WAITING_TIME_TO_CHECK_MOVED_OBJECT_AS_MILLISECONDS);

            bool isObjectExist = withObject;

            if(isObjectExist && withObject.IsLanded)
            {
                return false;
            }

            return true;
        }

        #endregion
    }

}