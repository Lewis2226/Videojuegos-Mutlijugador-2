using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    //hacia donde apunte el character
    Vector3 desiredDirection;
    public float speed = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            desiredDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            float mag = desiredDirection.magnitude;

            if(mag > 0)
            {
                transform.forward = desiredDirection;
                transform.Translate(0, 0, speed * Time.deltaTime);
            }
            
        }
    }
}
