using System;
using UnityEngine;

public class Clickable : MonoBehaviour
{
    public Action<GameObject> OnClick;
    public Action OnMouseEntered;
    public Action OnMouseExited;

    public bool IsMouseOver => _isMouseOver;
    public void SetActive(bool active)
    {
        _active = active;
    }

    [SerializeField] LayerMask _raycastLayerMask;

    bool _active, _isMouseOver;
    Camera _camera;
    Vector3 _mousePosition;

    void Awake()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        if (!_active) return;
        _mousePosition = Input.mousePosition;
        DetectMouseOver();
        DetectMouseClick();
    }

    void DetectMouseOver()
    {
        var ray = _camera.ScreenPointToRay(_mousePosition);
        if (!Physics.Raycast(ray, out var hitInfo, 100f, _raycastLayerMask))
        {
            if (_isMouseOver) return;
            HandleMouseExit();
            return;
        }
        
        if (hitInfo.collider.gameObject != gameObject)
        {
            if (_isMouseOver) return;
            HandleMouseExit();
            return;
        }

        if (!_isMouseOver) return;
        HandleMouseEntered();
    }

    void HandleMouseEntered()
    {
        _isMouseOver = true;
        OnMouseEntered?.Invoke();
    }

    void HandleMouseExit()
    {
        _isMouseOver = false;
        OnMouseExited?.Invoke();
    }

    void DetectMouseClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (!IsMouseOver) 
        {
            return;
        }
        OnClick?.Invoke(gameObject);
    }
}
