using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private Grid3D gridRef;
    [SerializeField] private int instancesPerTest;

    struct Path
    {
        public Vector3 startPos;
        public Vector3 targetPos;
    }

    private List<Path> paths;

    void Start()
    {
        paths = GenerateRandomPaths(instancesPerTest);
    }

    List<Path> GenerateRandomPaths(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 startingPos = RandomValidPos();
            Vector3 targetPos = RandomValidPos();

            Path path = new Path
            {
                startPos = startingPos,
                targetPos = targetPos
            };

            paths.Add(path);
        }

        return paths;
    }

    Vector3 RandomValidPos()
    {
        // Falta por hacer
        return Vector3.zero;
    }

    void Update()
    {
        
    }
}
