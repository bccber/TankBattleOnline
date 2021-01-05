using UnityEngine;

public class Bullet : MonoBehaviour
{
    public bool IsPlayerBullet;
    private float MoveSpeed = 12;

    void Update()
    {
        transform.Translate(transform.up * MoveSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;
        switch (tag)
        {
            case "Tank":
                if (!IsPlayerBullet)
                {
                    collision.SendMessage("Die");
                }
                break;

            case "Heart":
                Destroy(gameObject);
                collision.SendMessage("Die");
                break;

            case "Wall":
                {
                    Destroy(gameObject);
                    Destroy(collision.gameObject);
                }
                break;

            case "Enemy":
                if (IsPlayerBullet)
                {
                    Destroy(gameObject);
                    collision.SendMessage("Die");
                }
                break;

            case "Barrier":
            case "BoundsCollider":
                Destroy(gameObject);
                break;
        }
    }
}