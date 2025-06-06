using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class RobotSorter : MonoBehaviour
{
    public float accuracy = 0;
    public float spawnSpeed = 5;
    public bool spawning = false;

    [SerializeField] Transform _startPoint;

    [SerializeField] Transform _type1Point;
    [SerializeField] Transform _type2Point;
    [SerializeField] Transform _type3Point;

    [SerializeField] List<GameObject> _type1Parts;
    [SerializeField] List<GameObject> _type2Parts;
    [SerializeField] List<GameObject> _type3Parts;

    float _timer = 0f;
    List<Vector3> positions;

    void Start()
    {
        Random.InitState(DateTime.Now.Millisecond);

        positions = new() { _type1Point.position, _type2Point.position, _type3Point.position };
    }

    void Update()
    {
        if (!spawning) return;

        if (_timer < spawnSpeed) _timer += Time.deltaTime;
        else
        {
            SpawnPart();
            _timer = 0f;
        }

    }

    GameObject GetRandomPart()
    {
        var part = _type1Parts[0];

        var allParts = _type1Parts.Concat(_type2Parts).Concat(_type3Parts);

        part = allParts.ElementAt(Random.Range(0, allParts.Count()));

        return part;
    }

    void SpawnPart()
    {
        var part = GetRandomPart();

        Instantiate(part, _startPoint.position, Quaternion.identity);
    }

    void MoveToLane(RobotType robot)
    {
        var rand = Random.Range(0f, 1f);

        if (rand > accuracy)
        {
            robot.transform.position = positions[Random.Range(0, positions.Count())];
            return;
        }

        switch (robot.type)
        {
            case RobotType.robotType.Type1:
                robot.transform.position = positions[0];
                break;
            case RobotType.robotType.Type2:
                robot.transform.position = positions[1];
                break;
            case RobotType.robotType.Type3:
                robot.transform.position = positions[2];
                break;
        }
    }

    public void UpdateAccuracy(EvaluationData data)
    {
        spawning = true;
        accuracy = (float)data.correctPredictions / (float)data.finishedCycles;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.TryGetComponent(out RobotType robot))
        {
            MoveToLane(robot);
        }
    }
}
