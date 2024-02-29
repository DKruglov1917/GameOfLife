using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInput : MonoBehaviour
{
    [SerializeField] private InputActionReference _managePause, _clear, _lmb, _exit;
    [SerializeField] private GridManager _gridManager;
    private Camera _mainCamera;
    private bool _isMousePressed;

    private void Update()
    {
        if (_isMousePressed)
            Paint();
    }

    private void Paint()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit && hit.transform.TryGetComponent(out Tile tile))
        {
            tile.SetState(TileState.Alive);
        }
    }

    private void Clear(InputAction.CallbackContext context)
    {
        _gridManager.Clear(context);
    }

    private void ManagePause(InputAction.CallbackContext context)
    {
        _gridManager.ManagePause(context);
    }

    private void LMB(InputAction.CallbackContext context)
    {
        _isMousePressed = context.ReadValue<float>() == 1f;
    }

    private void Exit(InputAction.CallbackContext context)
    {
        Application.Quit();
    }

    private void OnEnable()
    {
        _mainCamera = Camera.main;

        _managePause.action.performed += ManagePause;
        _clear.action.performed += Clear;
        _lmb.action.performed += LMB;
        _exit.action.performed += Exit;
    }    

    private void OnDisable()
    {
        _managePause.action.performed -= ManagePause;
        _clear.action.performed -= Clear;
        _lmb.action.performed -= LMB;
        _exit.action.performed -= Exit;
    }
}
