using UnityEngine;

public class MinimapCamController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    void Start()
    {
        if (player == null)
        {
            player = GameManager.Instance.player;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
    }
}
