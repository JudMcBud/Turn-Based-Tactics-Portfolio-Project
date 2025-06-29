using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera.main.transform.position = new Vector3(5, 10, -5);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
    }
}
