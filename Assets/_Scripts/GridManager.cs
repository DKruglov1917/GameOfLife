using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int        _rows, _cols;
    [SerializeField] private float      _tickTime = .1f;
    [SerializeField] private GameObject _grid;
    [SerializeField] private Transform  _cam;
    [SerializeField] private Tile       _tilePrefab;
    [SerializeField] private GameObject _pauseText;

    private Dictionary<Vector2, Tile> _tiles;

    private float _cellSizeX, _cellSizeY;
    private bool _isPause = true;

    private IEnumerator _gameCoroutine;


    private void Start()
    {
        InitSetup();
        GenerateGrid();
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }

        Debug.LogError($"Tried to get tile at {pos} position");
        return null;
    }

    private void InitSetup()
    {
        _cellSizeX = _tilePrefab.transform.localScale.x;
        _cellSizeY = _tilePrefab.transform.localScale.y;
        _gameCoroutine = UpdateGridCoroutine();
    }

    private void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();

        for (int x = 0; x < _rows; x++)
        {
            for (int y = 0; y < _cols; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(_cellSizeX * x, _cellSizeY * y), Quaternion.identity, _grid.transform);
                spawnedTile.name = $"Tile {x} {y}";

                //check if x/y is even and then paint the tile
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);                
                spawnedTile.Init(isOffset);


                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        AssignNeighbors();

        Vector3 gridCenter = new Vector3((_rows - 1) * _cellSizeX / 2, (_cols - 1) * _cellSizeY / 2, -10);
        _cam.transform.position = gridCenter;

        void AssignNeighbors()
        {
            foreach (var tile in _tiles)
            {
                int x = (int)tile.Key.x;
                int y = (int)tile.Key.y;

                // Check neighbors to the left and right (cyclically along x)
                int leftX = (x - 1 + _cols) % _cols;
                int rightX = (x + 1) % _cols;
                tile.Value.Neighbors.Add(GetTileAtPosition(new Vector2(leftX, y)));
                tile.Value.Neighbors.Add(GetTileAtPosition(new Vector2(rightX, y)));

                // Check neighbors above and below (cyclically along y)
                int upY = (y - 1 + _rows) % _rows;
                int downY = (y + 1) % _rows;
                tile.Value.Neighbors.Add(GetTileAtPosition(new Vector2(x, upY)));
                tile.Value.Neighbors.Add(GetTileAtPosition(new Vector2(x, downY)));

                // Check diagonal neighbors (cyclically along both x and y)
                tile.Value.Neighbors.Add(GetTileAtPosition(new Vector2(leftX, upY)));
                tile.Value.Neighbors.Add(GetTileAtPosition(new Vector2(leftX, downY)));
                tile.Value.Neighbors.Add(GetTileAtPosition(new Vector2(rightX, upY)));
                tile.Value.Neighbors.Add(GetTileAtPosition(new Vector2(rightX, downY)));
            }
        }
    }

    private void DetermineNextLiveState()
    {
        int aliveNeighbors;

        foreach (var tile in _tiles)
        {
            aliveNeighbors = 0;
            CheckLiveNeighbors(tile.Value);

            CheckLonelinessDeath(tile.Value);
            CheckSurvive(tile.Value);
            CheckOvercrowdingDeath(tile.Value);
            CheckBecomeAlive(tile.Value);
        }

        void CheckLiveNeighbors(Tile tile)
        {            
            foreach (var neighbor in tile.Neighbors)
            {
                if (neighbor.state == TileState.Alive)
                    aliveNeighbors++;
            }
        }

        /// <summary>
        /// Any live cell with fewer than two live neighbors dies, as if by underpopulation.
        /// </summary>
        void CheckLonelinessDeath(Tile tile)
        {
            if (tile.state == TileState.Dead) return;

            if (aliveNeighbors < 2)
                tile.isAliveNext = false;
        }

        /// <summary>
        /// Any live cell with two or three live neighbors lives on to the next generation.
        /// </summary>
        void CheckSurvive(Tile tile)
        {
            if (tile.state == TileState.Dead) return;

            if (aliveNeighbors == 2 || aliveNeighbors == 3)
                tile.isAliveNext = true;
        }

        /// <summary>
        /// Any live cell with more than three live neighbors dies, as if by overpopulation.
        /// </summary>
        void CheckOvercrowdingDeath(Tile tile)
        {
            if (tile.state == TileState.Dead) return;

            if(aliveNeighbors > 3)
                tile.isAliveNext = false;
        }

        /// <summary>
        /// Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction.
        /// </summary>
        void CheckBecomeAlive(Tile tile)
        {
            if (tile.state == TileState.Alive) return;

            if (aliveNeighbors == 3)
                tile.isAliveNext = true;
        }
    }

    private void UpdateGrid()
    {
        foreach (var tile in _tiles)
        {
            if(tile.Value.isAliveNext)
                tile.Value.SetState(TileState.Alive);
            else
                tile.Value.SetState(TileState.Dead);
        }
    }

    public void ManagePause(InputAction.CallbackContext context)
    {
        if (_isPause)
        {
            StartCoroutine(_gameCoroutine);
            _isPause = false;
        }
        else
        {
            StopCoroutine(_gameCoroutine);
            _isPause = true;
        }

        _pauseText.gameObject.SetActive(_isPause);
    }

    public void Clear(InputAction.CallbackContext context)
    {
        StopCoroutine(_gameCoroutine);

        foreach (var tile in _tiles)
        {
            tile.Value.SetState(TileState.Dead);
        }

        _isPause = true;    
        _pauseText.gameObject.SetActive(_isPause);
    }

    private IEnumerator UpdateGridCoroutine()
    {
        while (true)
        {
            DetermineNextLiveState();
            UpdateGrid();
            yield return new WaitForSeconds(_tickTime);
        }
    }
}
