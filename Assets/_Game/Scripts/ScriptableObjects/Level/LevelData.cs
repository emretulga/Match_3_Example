using System.Collections.Generic;
using UnityEngine;

namespace EmreTulga.MatchThreeExample.Runtime.Core
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData", order = 1)]
    public class LevelData : ScriptableObject
    {
        #region VARIABLES
        public float CellSize, CellDistance;
        public Vector2 CellStartPoint;

        public float MatchObjectCreatingWorldPositionY;

        [SerializeField]
        public List<MatchObjectList> matchObjectMatrix;

        #endregion
    }
}
