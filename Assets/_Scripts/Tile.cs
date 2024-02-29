using System.Collections.Generic;
using UnityEngine;

public enum TileState
{
    Dead,
    Alive    
}

public class Tile : MonoBehaviour
{    
    public TileState state { get; private set; }

    public List<Tile> Neighbors = new List<Tile>();

    public bool isAliveNext;

    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private Color _occupiedColor;
    [SerializeField] private GameObject _highlight;

    private SpriteRenderer _renderer;
    private bool _isOffset;   
    

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Init(bool isOffset)
    {
        _isOffset = isOffset;
        _renderer.color = _isOffset ? _offsetColor : _baseColor;
        SetState(TileState.Dead);
    }

    public void SetState(TileState _state)
    {
        isAliveNext = false;
        state = _state;
        ManageColor();
    }

    private void ManageColor()
    {
        switch (state)
        {
            case (TileState.Dead):
                if (_isOffset)
                    _renderer.color = _offsetColor;
                else
                    _renderer.color = _baseColor;
                break;

            case (TileState.Alive):
                _renderer.color = _occupiedColor;
                break;
        }
    }

    private void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        _highlight.SetActive(false);
    }
}
