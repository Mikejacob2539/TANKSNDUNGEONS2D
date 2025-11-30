using UnityEngine;

public class DestroyAfterImages : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    SpriteRenderer sr;
    Color color;
    float fadeSpeed = 0.05f;
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        color = sr.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (color.a > 0)
        {
            color.a -= fadeSpeed;
            sr.color = color;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
