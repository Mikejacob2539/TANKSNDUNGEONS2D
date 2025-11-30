using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform playerTransform;
    public Transform turret;
    [SerializeField] Vector3 offset;
    [SerializeField] float damping;
    [SerializeField] Vector2 minPositions;
    [SerializeField] Vector2 maxpositions;
    [SerializeField] Transform Shaker;
    public float lookahead = 2f;
    public float damping2 = 0.25f;

    [SerializeField] Texture2D mouseTexture;
    float timer;


    private Vector3 vect = Vector3.zero;

    void Start()
    {

        Cursor.SetCursor(mouseTexture, Vector2.up, CursorMode.ForceSoftware);

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (playerTransform != null)
        {
            Vector3 desiredPos = playerTransform.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref vect, damping);


            Vector3 targetPos = playerTransform.position + offset;
            Vector3 aimDir = turret.up;
            targetPos += aimDir * lookahead;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref vect, damping2);
            float clampedX = Mathf.Clamp(transform.position.x, minPositions.x, maxpositions.x);
            float clampedY = Mathf.Clamp(transform.position.y, minPositions.y, maxpositions.y);
            transform.position = new Vector3(clampedX, clampedY, -10);
        }
    }

    public IEnumerator cameraShake(float duration, float magnitude)
    {
        float elaspedTime = 0f;
        Vector3 originalPos = Shaker.position;
        while (elaspedTime < duration)
        {
            float randomPosX = Random.Range(-1, 1) * magnitude;
            float randomPosY = Random.Range(-1, 1) * magnitude;
            Shaker.position = new Vector3(randomPosX, randomPosY, originalPos.z);
            elaspedTime += Time.deltaTime;
            yield return null;
        }
        Shaker.position = originalPos;
    }




}
