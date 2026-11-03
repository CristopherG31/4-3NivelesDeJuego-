using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    public Transform objetivo;                                   // el Jugador
    public Vector3 alturaObjetivo = new Vector3(0f, 1.5f, 0f);

    [Header("Distancia y zoom")]
    public float distancia = 8f;
    public float distanciaMin = 3f;
    public float distanciaMax = 15f;
    public float velocidadZoom = 5f;

    [Header("Rotación")]
    public float sensibilidad = 3f;              // mouse (click derecho)
    public float sensibilidadJoystick = 120f;    // grados por segundo con el joystick
    public float anguloMin = 10f;
    public float anguloMax = 70f;

    [Header("Móvil")]
    public Joystick joystickCamara;              // arrastra JoystickCamara

    private float yaw;
    private float pitch = 25f;

    void Start()
    {
        if (objetivo != null)
            yaw = objetivo.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        float giroX = 0f;
        float giroY = 0f;

        // PC: click derecho + mouse
        if (Input.GetMouseButton(1))
        {
            giroX += Input.GetAxis("Mouse X") * sensibilidad;
            giroY += Input.GetAxis("Mouse Y") * sensibilidad;
        }

        // Móvil: joystick de cámara
        if (joystickCamara != null)
        {
            giroX += joystickCamara.Horizontal * sensibilidadJoystick * Time.deltaTime;
            giroY += joystickCamara.Vertical * sensibilidadJoystick * Time.deltaTime;
        }

        yaw += giroX;
        pitch -= giroY;
        pitch = Mathf.Clamp(pitch, anguloMin, anguloMax);

        // Zoom con la rueda del mouse (solo PC)
        distancia -= Input.GetAxis("Mouse ScrollWheel") * velocidadZoom;
        distancia = Mathf.Clamp(distancia, distanciaMin, distanciaMax);

        Quaternion rotacion = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 foco = objetivo.position + alturaObjetivo;

        transform.position = foco - rotacion * Vector3.forward * distancia;
        transform.rotation = rotacion;
    }
}