using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{
    #region General Variables
    [Header("General references")]
    [SerializeField] Camera fpsCam; //Ref si disparamos desde el cenro de la cam
    [SerializeField] Transform shootPoint; //Ref si queremos disparar desde la punta del cañón.
    [SerializeField] LayerMask impactLayer; //Layer con la que el raycast interactúa.
    RaycastHit hit; //Almacén de la información de los objetos a los que el raycast puede impactar

    [Header("Weapon parameter")]
    [SerializeField] int damage = 10; //Daño del arma por bala.
    [SerializeField] float range = 100f; //Alcance del arma.
    [SerializeField] float spread = 0; //Radio de dispersión del arma.
    [SerializeField] float shootingCooldown = 0.2f; //Tiempo entre disparos.
    [SerializeField] float reloadTime = 1.5f; //Tiempo de recarga en segundos.
    [SerializeField] bool allowButtonHold = false; //Si el disparo se ejecuta por click (false) o mantener (true).

    [Header("Bullet Management")]
    [SerializeField] int ammoSize = 30; //Cant. máx de balas por cargador.
    [SerializeField] int bulletsPerTap = 1; //Cant de balas disparadas por cada ejecución de disparo
    int bulletsLeft; //Cantidad de balas dentro del cargador actual.

    [Header("Feedback References")]
    [SerializeField] GameObject impactEffect; //Ref al VFX de impacto de bala.

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting; //Indica si estamos disparando
    [SerializeField] bool canShoot; //Indica si podemos disparar en x momento del juego
    [SerializeField] bool reloading; //Indica si estamos recargando en x momento del juego.
    #endregion

    void Awake()
    {
        bulletsLeft = ammoSize; //Al iniciar el juego, el cargador está lleno.
        canShoot = true; //Al iniciar el juego, podemos disparar.
    }


    // Update is called once per frame
    void Update()
    {
        //Condición estricta de llamar a la rutina de disparo
        if (canShoot && shooting && !reloading && bulletsLeft > 0)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        //La corrutina se va a encargar de medir el tiempo entre disparos y la gestión del gasto de balas
        //Además, llamará al raycast de disparo que está definido en Shoot().

        canShoot = false; //Llave de seguridad que hace que si estamos disparando no podemos disparar
        if (!allowButtonHold) shooting = false; //Cerrar el bucle de disparo por pulsación.
        for (int i = 0; i < bulletsPerTap; i++)
        {
            if (bulletsLeft <= 0) break; //Segunda prevención de errores: Si no me quedan balas, no hago daño.
            Shoot(); //Llamada a Raycast que define el disparo
            bulletsLeft--; //Resta 1 
        }

        //Espera entre disparos
        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true; //Resetea la posibilidad de disparar.

    }

    void Shoot()
    {
        //Este es el método más importante
        //Aquí se define el disparo por raycast = utilizable con cualquier mecánica

        //Almacenar la direción de disparo y modificarla en caso de haber spread
        Vector3 direction = fpsCam.transform.forward; //Se lanza rayo hacia delante de la cámara
        //Añadir dispersión aleatoria según el valor de spread
        direction.x += Random.Range(-spread, spread); //Añadir dispersión aleatoria en el eje X
        direction.y += Random.Range(-spread, spread); //Añadir dispersión aleatoria en el eje X

        //DECLARACIÓN DEL RAYCAST
        //Physics.Raycast(Origen del rayo, dirección, almacén de la info del impacto, longitud del rayo, layer con la que impacta el rayo)
        if (Physics.Raycast(fpsCam.transform.position, direction, out hit, range, impactLayer))
        {
            //Aqui puedo codear todos los efectos que quiero para mi interacción
            Debug.Log(hit.collider.name);
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    void Reload()
    {
        if (bulletsLeft < ammoSize && !reloading) StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        reloading = true; //Estamos recargando por lo tanto no podemos regargar
        //AQUÍ LLAMARÍAMOS A LA ANIMACIÓN DE RECARGA 
        yield return new WaitForSeconds(reloadTime); //Esperar tanto tiempo como dure la animación de recarga
        bulletsLeft = ammoSize; //La cantidad de balas actuales se iguala a la máxima.
        reloading = false; //Termina la recarga, podemos volver a recargar.
    }


    #region Input Methods
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (allowButtonHold)
        {
            shooting = context.ReadValueAsButton(); //Detecta constantemente si el botón de disparo esta apretado
        }
        else
        {
            if (context.performed) shooting = true; //Shooting solo es true por pulsación.
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed) Reload();
    }
    #endregion
}
