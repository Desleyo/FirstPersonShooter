using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float sensitivity;
    [SerializeField] private float clampRotation;
    [SerializeField] Transform playerOrientation;

    private float xRotation;
    private float yRotation;

    [HideInInspector] public Vector3 currentRecoil;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        Look();
    }

    private void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        xRotation -= mouseY;
        yRotation += mouseX;

        //Clamp xRotation so the player can't go upside down
        xRotation = Mathf.Clamp(xRotation, -clampRotation, clampRotation);

        //Set the rotation of the camera (including the recoil), and the orientation object of the player
        transform.rotation = Quaternion.Euler(xRotation - currentRecoil.y, yRotation + currentRecoil.x, 0);
        playerOrientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void AddRecoil(Vector3 recoil)
    {
        currentRecoil = recoil;
    }
}
