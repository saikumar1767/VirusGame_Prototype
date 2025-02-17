using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    private Camera mainCam;
    public GameObject virusObject;
    private float distanceFromVirusObject = 1.0f;
    private Vector3 mousePos;
    public GameObject bullet;
    public Transform bulletTransform;
    public bool canFire;
    // how frequentlty player can fire
    private float timer;
    public float timeBetweenFiring;
    public static Shooting shooterInstance;

    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        shooterInstance = this;
    }

    void Update()
    {
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        Vector3 rotation = mousePos - transform.position;
        Vector3 directionToMouse = mousePos - virusObject.transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        // float rotZ = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
        // Rotate the red dot to face the mouse cursor.
        transform.rotation = Quaternion.Euler(0, 0, rotZ);
        // Set the position of the small circle relative to the rectangular object.
        transform.position = virusObject.transform.position + directionToMouse.normalized * distanceFromVirusObject;

        if (!canFire)
        {
            timer += Time.deltaTime;
            if (timer > timeBetweenFiring)
            {
                canFire = true;
                timer = 0;
            }
        }
        if (Input.GetMouseButton(0) && canFire)
        {
            canFire = false;
            Instantiate(bullet, bulletTransform.position, Quaternion.identity);
        }
        //Code to switch the shooting agent on right click of infected human
        HandleRightClick();
    }

    public void NotifyHit()
    {
        // Deactivate the shooter object upon a successful hit
        gameObject.SetActive(false);
        Destroy(gameObject);

    }


    private void HandleRightClick()
    {

        if (Input.GetMouseButtonDown(1)) // 1 is for right click
        {

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider != null)
            {

                bool hasInitialVirusChild = false;
                Transform targetVirusTransform = null;

                foreach (Transform child in hit.collider.transform)
                {
                    if (child.CompareTag("InitialVirus"))
                    {
                        hasInitialVirusChild = true;
                        targetVirusTransform = child;
                        break;
                    }
                }

                if (hasInitialVirusChild || hit.collider.CompareTag("InitialVirus"))
                {

                    if (!hasInitialVirusChild)
                        targetVirusTransform = hit.collider.transform;

                    Transform parentTransform = targetVirusTransform.parent;


                    if (parentTransform != null &&
                       (parentTransform.CompareTag("NVHuman1") || parentTransform.CompareTag("NVHuman2") || parentTransform.CompareTag("NVHuman3") || parentTransform.CompareTag("NVHuman4")))
                    {
                        Shooting shootingScript = null;

                        foreach (Transform virusChild in parentTransform)
                        {
                            if (virusChild.CompareTag("InitialVirus"))
                            {
                                foreach (Transform potentialRotatePoint in virusChild)
                                {

                                    if (potentialRotatePoint.name.StartsWith("Rotate Point"))
                                    {
                                        Debug.Log("Found Rotate Point under: " + virusChild.name);
                                        shootingScript = potentialRotatePoint.GetComponent<Shooting>();
                                        break;
                                    }
                                }
                            }

                            //If it doesn't have a RotatePoint child, instantiate one and attach Shooting script
                            if (shootingScript == null)
                            {
                                Debug.Log("creating new shooting agent");
                                GameObject newRotatePoint = Instantiate(gameObject, targetVirusTransform.position, Quaternion.identity);
                                newRotatePoint.transform.SetParent(targetVirusTransform);
                                shootingScript = newRotatePoint.GetComponent<Shooting>();
                                shootingScript.virusObject = parentTransform.gameObject;
                                Destroy(gameObject);
                            }

                            if (shootingScript != null)
                            {
                                Debug.Log(" no need of creating new shooting agent");
                                shootingScript.ActivateShooter();
                            }
                            else
                            {
                                Debug.Log("Failed to activate Shooting script for: " + targetVirusTransform.name);
                            }
                        }
                    }
                }

            }
        }
    }
    public void ActivateShooter()
    {
        // Activate your Shooting functionality here
        Debug.Log("Activating Shooter on: " + gameObject.name);
        canFire = true;
        this.enabled = true;
    }

}
