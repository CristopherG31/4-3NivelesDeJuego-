using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 6f;
    public Joystick joystick;          // JoystickMovimiento
    public Joystick joystickCamara;    // JoystickCamara

    private Rigidbody rb;
    private Transform cam;
    private bool enSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
    }
void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
        Saltar();
}
    void FixedUpdate()
    {
        float h = 0f;
        float v = 0f;

        if (joystick != null)
        {
            h = joystick.Horizontal;
            v = joystick.Vertical;
        }

        // Respaldo con teclado para PC
        if (h == 0f && v == 0f)
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
        }

        // Direcciones según la cámara (sin inclinación vertical)
        Vector3 adelante = cam.forward;
        adelante.y = 0f;
        adelante.Normalize();

        Vector3 derecha = cam.right;
        derecha.y = 0f;
        derecha.Normalize();

        Vector3 mov = adelante * v + derecha * h;
        if (mov.sqrMagnitude > 1f) mov.Normalize();

        rb.linearVelocity = new Vector3(mov.x * velocidad, rb.linearVelocity.y, mov.z * velocidad);

        // ¿Está girando la cámara? (click derecho o joystick de cámara)
        bool girandoCamara = Input.GetMouseButton(1) ||
            (joystickCamara != null && joystickCamara.Direction.sqrMagnitude > 0.01f);

        if (girandoCamara)
        {
            // Estilo MMO: el personaje mira hacia donde mira la cámara
            transform.rotation = Quaternion.Euler(0f, cam.eulerAngles.y, 0f);
        }
        else if (mov.sqrMagnitude > 0.01f)
        {
            // Si no giras la cámara, mira hacia donde camina
            transform.forward = mov;
        }
    }

    public void Saltar()
    {
        if (enSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enSuelo = false;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Suelo"))
            enSuelo = true;
    }
}