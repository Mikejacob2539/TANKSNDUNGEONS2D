using UnityEngine;
using UnityEngine.InputSystem;

public class Turrent_Rotation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayerInputActions2 playerControls;
    [SerializeField] PlayerInput scheme;

    void Awake()
    {
        playerControls = new PlayerInputActions2();
        playerControls.Player.Enable();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    void FixedUpdate()
    {
        // var looking = playerControls.Player.Look.ReadValue<Vector2>();
        if (scheme.currentControlScheme == "Joystick" || scheme.currentControlScheme == "Gamepad")
        {
            Debug.Log("Using Gamepad");
            Vector2 looking = playerControls.Player.Look.ReadValue<Vector2>();
            //Debug.Log(looking);
            if (looking.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(looking.y, looking.x) * Mathf.Rad2Deg - 90f;

                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            }
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            Vector2 lookDir = (mousePos - transform.position).normalized;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        }
    }
}
