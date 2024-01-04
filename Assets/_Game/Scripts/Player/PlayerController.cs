using EmreTulga.MatchThreeExample.Runtime.InputModule;
using UnityEngine;

namespace EmreTulga.MatchThreeExample.Runtime.Core
{
    public class PlayerController : MonoBehaviour
    {
        #region CONSTS

        private const string HORIZONTAL_INPUT_AXIS_NAME = "Mouse X";
        private const string VERTICAL_INPUT_AXIS_NAME = "Mouse Y";

        #endregion

        #region VARIABLES
        public static PlayerController Instance;

        private MatchObject _selectedObject;

        #endregion

        #region FUNCTIONS

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            CheckInteractingMatchObjectUpdate();
        }

        private void CheckInteractingMatchObjectUpdate()
        {
            bool isSelectedObjectExists = _selectedObject;

            if(!isSelectedObjectExists || !_selectedObject.IsLanded)
            {
                return;
            }

            float inputSense = InputManager.INPUT_SENSE;

            Vector2Int objectPosition = _selectedObject.CurrentPosition;

            bool isInteractingWithSelectedObject = false;

            float horizontalInputAxis = Input.GetAxis(HORIZONTAL_INPUT_AXIS_NAME);
            float verticalInputAxis = Input.GetAxis(VERTICAL_INPUT_AXIS_NAME);

            if(Input.GetMouseButtonUp(0))
            {
                isInteractingWithSelectedObject = true;
            }
            else if(Mathf.Abs(verticalInputAxis) > inputSense)
            {
                int verticalDirection =  (int)Mathf.Sign(verticalInputAxis);
                _selectedObject.LandToPosition(objectPosition + Vector2Int.up * verticalDirection, true);

                isInteractingWithSelectedObject = true;
            }
            else if(Mathf.Abs(horizontalInputAxis) > inputSense)
            {
                int horizontalDirection =  (int)Mathf.Sign(horizontalInputAxis);
                _selectedObject.LandToPosition(objectPosition + Vector2Int.right * horizontalDirection, true);

                isInteractingWithSelectedObject = true;
            }

            if(isInteractingWithSelectedObject)
            {
                ReleaseSelectedObject();
            }
        }

        public void SelectObject(MatchObject matchObject)
        {
            _selectedObject = matchObject;
        }

        public void ReleaseSelectedObject()
        {
            _selectedObject = null;
        }

        #endregion
    }

}