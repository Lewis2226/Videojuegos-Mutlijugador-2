using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    //hacia donde apunte el character
    Vector3 desiredDirection;
    public float speed = 1.0f;
    float characetSpeed = 0f;

    //salud del jugador
    //public int health = 100;

    [Header("Camera")]
    public Vector3 CameraOffset = new Vector3(0, 4,-3);
    public Vector3 CameraViewOffset = new Vector3(0, 1.5f,0);
    Camera cam;

    //networkvariable para replicar la vida
    NetworkVariable<int> health = new NetworkVariable<int> (100,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private UiManager hud;
    private GameManager gameManager;

    [Header("SFX")]
    public AudioClip DamageSound;
    public AudioClip DeathSound;
    AudioSource audioSource;
    Animator animator;

    public override void OnNetworkSpawn()
    {
        Debug.Log("Hola mundo soy un " + (IsClient? "cliente" : "servidor"));
        Debug.Log("IsClient = " + IsClient + ", IsServer = " + IsServer + ", IsHost = " + IsHost);
        Debug.Log(name + " Is owner " + IsOwner);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hud = GameObject.Find("GameManager").GetComponent<UiManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        transform.position = gameManager.GetSpawnPoint();

        //asignar la camara
        if (IsOwner)
        {
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            cam.transform.position = transform.position + CameraOffset;
            cam.transform.LookAt(transform.position + CameraViewOffset);
        }

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = transform.position;
        if (IsOwner)
        {
            desiredDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            desiredDirection.Normalize();

            if (IsAlive())
            {
                float mag = desiredDirection.magnitude;

                if (mag > 0)
                {
                    //transform.forward = desiredDirection;
                    //Interpolar entre la rotación actual y la desesda

                    Quaternion q = Quaternion.LookRotation(desiredDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 10);
                    transform.Translate(0, 0, speed * Time.deltaTime);
                    
                }
                else
                {
                   
                }
                

                //Prueba del sistema, de vida
                if (Input.GetKeyDown(KeyCode.T))
                {
                    TakeDamage(5);
                }
                
            }

            //actualizar la camara
            cam.transform.position = transform.position + CameraOffset;
            cam.transform.LookAt(transform.position + CameraViewOffset);

            //actualizar Hud
            hud.labelHealth.text = health.Value.ToString();

        }
        characetSpeed = (transform.position - currentPosition).magnitude / Time.deltaTime;
        Debug.Log("Del jugador " +  name + characetSpeed);
        animator.SetFloat("movement", characetSpeed);
    }


    //RPC= 
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int amount)
    {
        Debug.Log("RPC recibido TakeDamage");
        TakeDamage(amount);
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive()) return;


        if (!IsServer) 
        {
          TakeDamageRpc(amount);
        }
        else
        {
            health.Value -= amount;

            if (health.Value <= 0)
            {
                health.Value = 0;

                if (!IsAlive())
                {
                    OnDeath();
                }
            }
            else
            {
                audioSource.clip = DamageSound;
                audioSource.Play();
            }
        }     
    }

    public void OnDeath()
    {
        //efectos
        Debug.Log(name + " me muero");
        audioSource.clip = DeathSound;
        audioSource.Play();
        animator.SetBool("dead", true);
        animator.SetFloat("movement", 0f);
        
        
    }

    public bool IsAlive()
    {
        return health.Value > 0;
    }
}
