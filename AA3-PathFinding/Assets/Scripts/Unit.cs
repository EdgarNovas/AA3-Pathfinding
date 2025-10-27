using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float speed = 5f; // Velocidad de movimiento

    // Variables de Pathfinding A*
    private Vector3[] path;
    private int targetIndex;

    // Componentes
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Esta función se llama en cada frame.
    /// La usamos para detectar el input del usuario.
    /// </summary>
    private void Update()
    {
        // Detectar si el usuario ha hecho clic con el botón izquierdo (0)
        if (Input.GetMouseButtonDown(0))
        {
            // Obtener la posición del ratón en la pantalla y convertirla a una posición en el mundo 2D
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = transform.position.z; // Asegurarse de que la Z sea la misma que la unidad

            // Solicitar un camino a nuestro sistema A* hacia la posición del clic
            PathRequestManager.RequestPath(transform.position, targetPosition, OnPathFound);
        }
    }

    /// <summary>
    /// Callback que recibe el camino desde el PathRequestManager.
    /// </summary>
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful && newPath.Length > 0)
        {
            path = newPath;
            targetIndex = 0; // Reiniciamos el índice para empezar a seguir el nuevo camino
        }
        else
        {
            path = null; // No se encontró camino
        }
    }

    /// <summary>
    /// Esta función se llama en el bucle de físicas.
    /// La usamos para el movimiento.
    /// </summary>
    private void FixedUpdate()
    {
        // 1. LÓGICA DE MOVIMIENTO: Seguir el camino A*
        Vector3 velocity = HandleMovement();
    }

    /// <summary>
    /// Mueve la unidad a lo largo de los waypoints del 'path' y devuelve la velocidad actual.
    /// </summary>
    Vector3 HandleMovement()
    {
        // Si no hay camino, no nos movemos.
        if (path == null || path.Length == 0)
        {
            return Vector3.zero;
        }

        // Obtener el waypoint actual
        Vector3 currentWaypoint = path[targetIndex];
        Vector3 oldPos = transform.position;

        // Moverse hacia el waypoint
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.fixedDeltaTime);

        // Calcular la velocidad real para la animación
        Vector3 velocity = (transform.position - oldPos) / Time.fixedDeltaTime;

        // Comprobar si hemos llegado al waypoint
        if (Vector3.Distance(transform.position, currentWaypoint) < 0.01f)
        {
            targetIndex++; // Ir al siguiente waypoint
            if (targetIndex >= path.Length)
            {
                path = null; // Hemos llegado al final del camino
            }
        }

        return velocity;
    }

    /// <summary>
    /// Actualiza el Animator y gira el SpriteRenderer en función de la velocidad.
    /// </summary>
    void HandleAnimationAndFlipping(Vector3 velocity)
    {
        // --- Animación ---
        animator.SetBool("IsWalking", velocity.magnitude > 0.1f);

        // --- Giro del Sprite ---
        float horizontalDirection = velocity.x;

        // Si no nos movemos horizontalmente, miramos hacia el siguiente waypoint
        // (si es que existe)
        if (Mathf.Abs(horizontalDirection) < 0.1f && path != null && path.Length > 0)
        {
            horizontalDirection = path[targetIndex].x - transform.position.x;
        }

        // Aplicar el giro
        if (horizontalDirection > 0.1f)
        {
            spriteRenderer.flipX = true;
        }
        else if (horizontalDirection < -0.1f)
        {
            spriteRenderer.flipX = false;
        }
    }

    // OnDrawGizmos sigue siendo útil para depurar el camino
    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one * 0.5f);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}