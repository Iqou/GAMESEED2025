using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public GameObject player;
    public Vector3 offset = new Vector3(0, 9, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.position = player.transform.position + offset;
    }
}
