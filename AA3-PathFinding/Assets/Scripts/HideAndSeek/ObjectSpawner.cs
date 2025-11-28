using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{

    Camera cam;            // Cámara desde donde se lanza el raycast
    public GameObject prefab;     // Prefab a instanciar
    public LayerMask obstacleLayer; // LayerMask para detectar suelo y objetos destructibles

    private void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TrySpawn();

        if (Input.GetKeyDown(KeyCode.Q))
            TryDestroy();
    }

    void TrySpawn()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Raycast without any layer filter first
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // If the thing hit is NOT in obstacle layer → spawn allowed
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) == 0)
            {
                Instantiate(prefab, hit.point, Quaternion.identity);
            }
        }
    }

    void TryDestroy()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // Only destroy if it IS in obstacle layer
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                Destroy(hit.collider.gameObject);
            }
        }
    }
}
