using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Heart : MonoBehaviour
{
    public Sprite BrokenSprite;
    public GameObject ExplosionPrefab;
    public AudioClip DieAudio;

    private SpriteRenderer _Sr;

    private void Awake()
    {
        _Sr = GetComponent<SpriteRenderer>();
    }

    public void Die()
    {
        Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
        _Sr.sprite = BrokenSprite;
        AudioSource.PlayClipAtPoint(DieAudio, transform.position);

        GameManager.Instance.GameOver();
    }
}
