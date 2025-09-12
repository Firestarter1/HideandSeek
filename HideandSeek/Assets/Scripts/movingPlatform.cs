using UnityEngine;

public class movingPlatform : MonoBehaviour
{
    [SerializeField] int speed;

    [SerializeField] Transform platform;
    [SerializeField] Transform destination;

    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPos = platform.position;
    }

    // Update is called once per frame
    void Update()
    {
        platform.transform.position += Vector3.MoveTowards(platform.position, destination.position, speed * Time.deltaTime);
    }
}
