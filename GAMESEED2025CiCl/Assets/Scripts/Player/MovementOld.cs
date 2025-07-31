using UnityEngine;

public class MovementOld : MonoBehaviour
{

    private float speed = 5.0f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       // Input Handler
       if (Input.GetMouseButtonDown(0))
        {
            // Buat RayCast mengikuti posisi mouse
            Ray rayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            // Kalau Raycast kena collider bakal nyimpan posisi target yang di hit
            if (Physics.Raycast(rayCast, out rayHit))
            {
                targetPosition = rayHit.point;
                targetPosition.y = transform.position.y;
                isMoving = true;
            }
        }

       // Movement Handler
       if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
            }
        }
    }
}
