
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 6f;

    [Header("Controles")]
    public Joystick joystick;
    public Joystick joystickCamara;

    private Rigidbody rb;
    private Transform cam;
    private bool enSuelo;

    // Guarda las superficies sobre las que está apoyado el jugador.
    private HashSet<Collider> superficiesSuelo = new HashSet<Collider>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void Update()
    {
        // Salto con teclado.
        if (Input.GetKeyDown(KeyCode.Space))
            Saltar();
    }

    void FixedUpdate()
    {
        if (cam == null)
            return;

        float h = 0f;
        float v = 0f;

        if (joystick != null)
        {
            h = joystick.Horizontal;
            v = joystick.Vertical;
        }

        // Respaldo con teclado para PC.
        if (h == 0f && v == 0f)
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
        }

        // Direcciones según la cámara, sin inclinación vertical.
        Vector3 adelante = cam.forward;
        adelante.y = 0f;
        adelante.Normalize();

        Vector3 derecha = cam.right;
        derecha.y = 0f;
        derecha.Normalize();

        Vector3 mov = adelante * v + derecha * h;

        if (mov.sqrMagnitude > 1f)
            mov.Normalize();

        rb.linearVelocity = new Vector3(
            mov.x * velocidad,
            rb.linearVelocity.y,
            mov.z * velocidad
        );

        // Control de rotación de cámara.
        bool girandoCamara =
            Input.GetMouseButton(1) ||
            (joystickCamara != null &&
             joystickCamara.Direction.sqrMagnitude > 0.01f);

        if (girandoCamara)
        {
            transform.rotation =
                Quaternion.Euler(0f, cam.eulerAngles.y, 0f);
        }
        else if (mov.sqrMagnitude > 0.01f)
        {
            transform.forward = mov;
        }

        // Actualiza el estado del suelo.
        superficiesSuelo.RemoveWhere(superficie => superficie == null);
        enSuelo = superficiesSuelo.Count > 0;
    }

    public void Saltar()
    {
        if (!enSuelo)
            return;

        // Elimina la velocidad vertical anterior para que el salto sea consistente.
        Vector3 velocidadActual = rb.linearVelocity;
        velocidadActual.y = 0f;
        rb.linearVelocity = velocidadActual;

        rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);

        enSuelo = false;
        superficiesSuelo.Clear();
    }

    void OnCollisionStay(Collision col)
    {
        // Solo considera suelo las superficies que están debajo del jugador.
        foreach (ContactPoint contacto in col.contacts)
        {
            if (Vector3.Dot(contacto.normal, Vector3.up) > 0.5f)
            {
                superficiesSuelo.Add(col.collider);
                break;
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        superficiesSuelo.Remove(col.collider);
    }

    void OnDisable()
    {
        superficiesSuelo.Clear();
        enSuelo = false;
    }
}
