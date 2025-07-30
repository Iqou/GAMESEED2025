using UnityEngine;

public class ToaRW : BaseHoreg
{
    public float coneAngle = 45.0f;
    public float duration = 1.5f;
    public bool isActive = false;

    private GameObject aoeInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horegName = "ToaRW";
        cooldownTime = 4.0f;
        weight = 2.0f;
        saweranMultiplier = 1.2f;
        aoeRadius = 1.0f;
        keyTrigger = KeyCode.W;
        color = Color.red;
        minimumDesibeOutput = 70.0f;
        maximumDesibelOutput = 80.0f;
    }


    public override void Attack(float currentTime, bool isOnBeat)
    {
        Update();
    }

    void Update()
    {
        if (Input.GetKeyDown(keyTrigger))
        {
            if (!isActive)
            {
                aoeInstance = Instantiate(aoePrefab, this.transform.position, Quaternion.identity);
                aoeInstance.transform.localScale = new Vector3(aoeRadius, transform.position.y, aoeRadius);
                TriggerAOE(true);
            }
            else
            {
                if(aoeInstance != null)
                {
                    Destroy(aoeInstance);
                }
            }

            if (isActive && aoeInstance != null)
            {
                Vector3 mousePos = GetMousePos();
                Vector3 direction = (mousePos - this.transform.position).normalized;

                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);

                aoeInstance.transform.position = transform.position;
                aoeInstance.transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
            }
        }
    }

    private Vector3 GetMousePos()
    {
        Ray rayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(rayCast, out float distance))
        {
            return rayCast.GetPoint(distance);
        }

        return transform.position + transform.forward * 5f;
    }

    protected override void TriggerAOE(bool isOnBeat)
    {
        desibelOutput = Random.Range(minimumDesibeOutput, maximumDesibelOutput);
        if (isOnBeat)
        {
            Debug.Log($"{gameObject.name} Dapat Bonus Beat");
        }
        Debug.Log("Toa RW ke Trigger");

        Destroy(aoeInstance, 2.0f);
    }


}
