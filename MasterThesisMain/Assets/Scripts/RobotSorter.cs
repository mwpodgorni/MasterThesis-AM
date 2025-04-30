using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        positions = new(){ _type1Point.position, _type2Point.position, _type3Point.position };
    }

    // Update is called once per frame
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

        var allParts = _type1Parts.Concat( _type2Parts).Concat(_type3Parts);

        part = allParts.ElementAt(Random.Range(0, allParts.Count()));

        return part;
    }

    void SpawnPart()
    {
        Debug.Log("spawnig");
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

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.TryGetComponent(out RobotType robot))
        {
            MoveToLane(robot);
        }
    }
}
