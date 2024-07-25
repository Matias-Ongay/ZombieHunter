using UnityEngine;
using System.Collections;
using UnityEngine.UI;  // Permite usar UI.
using UnityEngine.SceneManagement;

namespace Completed
{
    // Player hereda de MovingObject, nuestra clase base para objetos que pueden moverse. Enemy también hereda de esta clase.
    public class Player : MovingObject
    {
        public float restartLevelDelay = 1f;  // Tiempo de retraso en segundos para reiniciar el nivel.
        public int pointsPerFood = 10;  // Número de puntos a agregar a los puntos de comida del jugador al recoger un objeto de comida.
        public int pointsPerSoda = 20;  // Número de puntos a agregar a los puntos de comida del jugador al recoger un objeto de soda.
        public int wallDamage = 1;  // Cuánto daño hace un jugador a una pared cuando la corta.
        public Text foodText;  // Texto UI para mostrar el total actual de comida del jugador.
        public AudioClip moveSound1;  // Uno de los 2 clips de audio para reproducir cuando el jugador se mueve.
        public AudioClip moveSound2;  // Dos de los 2 clips de audio para reproducir cuando el jugador se mueve.
        public AudioClip eatSound1;  // Uno de los 2 clips de audio para reproducir cuando el jugador recoge un objeto de comida.
        public AudioClip eatSound2;  // Dos de los 2 clips de audio para reproducir cuando el jugador recoge un objeto de comida.
        public AudioClip drinkSound1;  // Uno de los 2 clips de audio para reproducir cuando el jugador recoge un objeto de soda.
        public AudioClip drinkSound2;  // Dos de los 2 clips de audio para reproducir cuando el jugador recoge un objeto de soda.
        public AudioClip gameOverSound;  // Clip de audio para reproducir cuando el jugador muere.

        private Animator animator;  // Se utiliza para almacenar una referencia al componente de animación del jugador.
        private int food;  // Se utiliza para almacenar el total de puntos de comida del jugador durante el nivel.
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;  // Se utiliza para almacenar la ubicación del origen del toque en la pantalla para los controles móviles.
#endif

        // Start anula la función Start de MovingObject
        protected override void Start()
        {
            // Obtiene una referencia al componente de animación del jugador.
            animator = GetComponent<Animator>();

            // Obtiene el total actual de puntos de comida almacenado en GameManager.instance entre niveles.
            food = GameManager.instance.playerFoodPoints;

            // Establece el foodText para reflejar el total actual de comida del jugador.
            foodText.text = "Comida: " + food;

            // Llama a la función Start de la clase base MovingObject.
            base.Start();
        }

        // Esta función se llama cuando el comportamiento se desactiva o se vuelve inactivo.
        private void OnDisable()
        {
            // Cuando el objeto Player se desactiva, almacena el total actual de comida local en el GameManager para que pueda recargarse en el siguiente nivel.
            GameManager.instance.playerFoodPoints = food;
        }

        private void Update()
        {
            // Si no es el turno del jugador, sale de la función.
            if (!GameManager.instance.playersTurn) return;

            int horizontal = 0;  // Se utiliza para almacenar la dirección de movimiento horizontal.
            int vertical = 0;  // Se utiliza para almacenar la dirección de movimiento vertical.

            // Comprueba si estamos ejecutando en el editor de Unity o en una compilación independiente.
#if UNITY_STANDALONE || UNITY_WEBPLAYER

            // Obtiene la entrada del gestor de entrada, redondea a un número entero y almacena en horizontal para establecer la dirección del eje x.
            horizontal = (int)(Input.GetAxisRaw("Horizontal"));

            // Obtiene la entrada del gestor de entrada, redondea a un número entero y almacena en vertical para establecer la dirección del eje y.
            vertical = (int)(Input.GetAxisRaw("Vertical"));

            // Comprueba si se está moviendo horizontalmente, si es así establece vertical en cero.
            if (horizontal != 0)
            {
                vertical = 0;
            }

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            
            // Comprueba si Input ha registrado más de cero toques.
            if (Input.touchCount > 0)
            {
                // Almacena el primer toque detectado.
                Touch myTouch = Input.touches[0];
                
                // Comprueba si la fase de ese toque es igual a Began.
                if (myTouch.phase == TouchPhase.Began)
                {
                    // Si es así, establece touchOrigin en la posición de ese toque.
                    touchOrigin = myTouch.position;
                }
                
                // Si la fase del toque no es Began, y en cambio es igual a Ended y la x de touchOrigin es mayor o igual a cero:
                else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
                {
                    // Establece touchEnd para que sea igual a la posición de este toque.
                    Vector2 touchEnd = myTouch.position;
                    
                    // Calcula la diferencia entre el inicio y el final del toque en el eje x.
                    float x = touchEnd.x - touchOrigin.x;
                    
                    // Calcula la diferencia entre el inicio y el final del toque en el eje y.
                    float y = touchEnd.y - touchOrigin.y;
                    
                    // Establece touchOrigin.x en -1 para que nuestra declaración else if evalúe falso y no se repita inmediatamente.
                    touchOrigin.x = -1;
                    
                    // Comprueba si la diferencia en el eje x es mayor que la diferencia en el eje y.
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                        // Si x es mayor que cero, establece horizontal en 1, de lo contrario, establécelo en -1.
                        horizontal = x > 0 ? 1 : -1;
                    else
                        // Si y es mayor que cero, establece vertical en 1, de lo contrario, establécelo en -1.
                        vertical = y > 0 ? 1 : -1;
                }
            }
            
#endif // Fin de la sección de compilación dependiente de la plataforma móvil iniciada arriba con #elif

            // Comprueba si tenemos un valor distinto de cero para horizontal o vertical.
            if (horizontal != 0 || vertical != 0)
            {
                // Llama a AttemptMove pasando el parámetro genérico Wall, ya que eso es lo que Player puede interactuar si encuentra uno (atacándolo).
                // Pasa horizontal y vertical como parámetros para especificar la dirección en la que mover al jugador.
                AttemptMove<Wall>(horizontal, vertical);
            }
        }

        // AttemptMove anula la función AttemptMove en la clase base MovingObject.
        // AttemptMove toma un parámetro genérico T que para Player será del tipo Wall, también toma enteros para la dirección x y y en la que moverse.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            // Cada vez que el jugador se mueve, resta puntos de comida.
            food--;

            // Actualiza la visualización del texto de comida para reflejar la puntuación actual.
            foodText.text = "Comida: " + food;

            // Llama al método AttemptMove de la clase base, pasando el componente T (en este caso Wall) y las direcciones x e y para moverse.
            base.AttemptMove<T>(xDir, yDir);

            // Hit nos permite hacer referencia al resultado del Linecast realizado en Move.
            RaycastHit2D hit;

            // Si Move devuelve true, lo que significa que Player pudo moverse a un espacio vacío.
            if (Move(xDir, yDir, out hit))
            {
                // Llama a RandomizeSfx de SoundManager para reproducir el sonido de movimiento, pasando dos clips de audio para elegir.
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }

            // Como el jugador se ha movido y ha perdido puntos de comida, comprueba si el juego ha terminado.
            CheckIfGameOver();

            // Establece el booleano playersTurn de GameManager en false ya que el turno del jugador ha terminado.
            GameManager.instance.playersTurn = false;
        }

        // OnCantMove anula la función abstracta OnCantMove en MovingObject.
        // Toma un parámetro genérico T que en el caso de Player es una Wall que el jugador puede atacar y destruir.
        protected override void OnCantMove<T>(T component)
        {
            // Establece hitWall para que sea igual al componente pasado como parámetro.
            Wall hitWall = component as Wall;

            // Llama a la función DamageWall de la Wall que estamos golpeando.
            hitWall.DamageWall(wallDamage);

            // Establece el trigger de ataque del controlador de animación del jugador para reproducir la animación de ataque del jugador.
            animator.SetTrigger("playerChop");
        }

        // OnTriggerEnter2D se envía cuando otro objeto entra en un collider de activación adjunto a este objeto (solo física 2D).
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Comprueba si la etiqueta del trigger con el que colisionó es Exit.
            if (other.tag == "Exit")
            {
                // Invoca la función Restart para comenzar el siguiente nivel con un retraso de restartLevelDelay (predeterminado 1 segundo).
                Invoke("Restart", restartLevelDelay);

                // Desactiva el objeto del jugador ya que el nivel ha terminado.
                enabled = false;
            }

            // Comprueba si la etiqueta del trigger con el que colisionó es Food.
            else if (other.tag == "Food")
            {
                // Agrega pointsPerFood al total actual de comida del jugador.
                food += pointsPerFood;

                // Actualiza foodText para representar el total actual y notificar al jugador que ganaron puntos.
                foodText.text = "+" + pointsPerFood + " Comida: " + food;

                // Llama a la función RandomizeSfx de SoundManager y pasa dos sonidos de comer para elegir y reproducir el efecto de sonido de comer.
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                // Desactiva el objeto de comida con el que el jugador colisionó.
                other.gameObject.SetActive(false);
            }

            // Comprueba si la etiqueta del trigger con el que colisionó es Soda.
            else if (other.tag == "Soda")
            {
                // Agrega pointsPerSoda al total de puntos de comida del jugador.
                food += pointsPerSoda;

                // Actualiza foodText para representar el total actual y notificar al jugador que ganaron puntos.
                foodText.text = "+" + pointsPerSoda + " Comida: " + food;

                // Llama a la función RandomizeSfx de SoundManager y pasa dos sonidos de beber para elegir y reproducir el efecto de sonido de beber.
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                // Desactiva el objeto de soda con el que el jugador colisionó.
                other.gameObject.SetActive(false);
            }
        }

        // Restart recarga la escena cuando se llama.
        private void Restart()
        {
            // Carga la última escena cargada, en este caso Main, la única escena en el juego. Y la cargamos en modo "Single" para que reemplace la existente y no cargue todos los objetos de la escena en la escena actual.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        // LoseFood se llama cuando un enemigo ataca al jugador.
        // Toma un parámetro loss que especifica cuántos puntos perder.
        public void LoseFood(int loss)
        {
            // Establece el trigger para el animador del jugador para la transición a la animación de jugador golpeado.
            animator.SetTrigger("playerHit");

            // Resta los puntos de comida perdidos del total del jugador.
            food -= loss;

            // Actualiza la visualización de la comida con el nuevo total.
            foodText.text = "-" + loss + " Comida: " + food;

            // Comprueba si el juego ha terminado.
            CheckIfGameOver();
        }

        // CheckIfGameOver comprueba si el jugador se ha quedado sin puntos de comida y, si es así, termina el juego.
        private void CheckIfGameOver()
        {
            // Comprueba si el total de puntos de comida es menor o igual a cero.
            if (food <= 0)
            {
                // Llama a la función PlaySingle de SoundManager y le pasa el gameOverSound como el clip de audio para reproducir.
                SoundManager.instance.PlaySingle(gameOverSound);

                // Detiene la música de fondo.
                SoundManager.instance.musicSource.Stop();

                // Llama a la función GameOver de GameManager.
                GameManager.instance.GameOver();
            }
        }
    }
}
