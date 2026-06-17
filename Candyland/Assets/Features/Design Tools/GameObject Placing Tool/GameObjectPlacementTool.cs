using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameObjectPlacementTool : MonoBehaviour
{
    private Vector3 _clickPosition;
    [SerializeField] private List<Vector3> _clickedPlaces;
    [SerializeField] private UnityEvent<Vector3> onFiveClicks;

    void OnMouseDown()
    {
        PlaceObjects();
    }

    void PlaceObjects()
    {
        if(_clickedPlaces.Count < 5)
        {           
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
            {         
                Vector3 clickWorldPosition = hitInfo.point;

                if(this != null && !_clickedPlaces.Contains(clickWorldPosition) && clickWorldPosition != _clickPosition)
                {
                    _clickedPlaces.Add(clickWorldPosition);
                    _clickPosition = clickWorldPosition;
                }
            }
        }
        else if(_clickedPlaces.Count == 5)
        {            
            onFiveClicks.Invoke(_clickPosition);
        }
        else if(_clickedPlaces.Count > 5)
        {
            _clickedPlaces.Clear();
        }
    }
}
