using UnityEngine;

public class Shooting : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootingPoint;
    [SerializeField] float bulletMoveSpeed = 24f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Shoot()
    {
        // Shooting logic would go here//
        if (CompareTag("Player"))
        {
            if (Input.GetButtonDown("Fire1"))
                ShootProjectiles();
        }
        else if (CompareTag("Enemy"))
        {
            Debug.Log("Enemy Pew Pew");
            ShootProjectiles();
        }


    }

    void ShootProjectiles()
    {
        Rigidbody2D body = Instantiate(bullet, shootingPoint.position, shootingPoint.rotation).GetComponent<Rigidbody2D>();
        body.AddForce(shootingPoint.up * bulletMoveSpeed, ForceMode2D.Impulse);
    }
}
