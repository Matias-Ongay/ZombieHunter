using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
    using System.Collections.Generic;       //Nos permite usar Listas.
    using UnityEngine.UI;                   //Nos permite usar UI.

    public class GameManager : MonoBehaviour
    {
        public float levelStartDelay = 2f;                      //Tiempo de espera antes de comenzar el nivel, en segundos.
        public float turnDelay = 0.1f;                          //Retraso entre cada turno del jugador.
        public int playerFoodPoints = 100;                      //Valor inicial de los puntos de comida del jugador.
        public static GameManager instance = null;              //Instancia estática de GameManager que permite que sea accesible por cualquier otro script.
        [HideInInspector] public bool playersTurn = true;       //Booleano para comprobar si es el turno del jugador, oculto en el inspector pero público.


        private Text levelText;                                 //Texto para mostrar el número del nivel actual.
        private GameObject levelImage;                          //Imagen para bloquear el nivel mientras se configura, fondo para el texto del nivel.
        private BoardManager boardScript;                       //Almacena una referencia a nuestro BoardManager que configurará el nivel.
        private int level = 1;                                  //Número del nivel actual, expresado en el juego como "Día 1".
        private List<Enemy> enemies;                            //Lista de todas las unidades Enemy, utilizada para emitirles comandos de movimiento.
        private bool enemiesMoving;                             //Booleano para comprobar si los enemigos se están moviendo.
        private bool doingSetup = true;                         //Booleano para comprobar si estamos configurando el tablero, evita que el jugador se mueva durante la configuración.


        //Awake siempre se llama antes de cualquier función Start
        void Awake()
        {
            //Comprueba si la instancia ya existe
            if (instance == null)
                //Si no es así, establece la instancia a esta
                instance = this;
            //Si la instancia ya existe y no es esta:
            else if (instance != this)
                //Entonces destruye esto. Esto refuerza nuestro patrón singleton, lo que significa que solo puede haber una instancia de GameManager.
                Destroy(gameObject);

            //Establece esto para que no se destruya al recargar la escena
            DontDestroyOnLoad(gameObject);

            //Asigna enemigos a una nueva lista de objetos Enemy.
            enemies = new List<Enemy>();

            //Obtiene una referencia de componente al script BoardManager adjunto
            boardScript = GetComponent<BoardManager>();

            //Llama a la función InitGame para inicializar el primer nivel 
            InitGame();
        }

        //Esto se llama solo una vez, y el parámetro indica que se debe llamar solo después de que se haya cargado la escena
        //(de lo contrario, nuestro callback de carga de escena se llamaría en la primera carga, y no queremos eso)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //Registra el callback para que se llame cada vez que se cargue la escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //Esto se llama cada vez que se carga una escena.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.level++;
            instance.InitGame();
        }


        //Inicializa el juego para cada nivel.
        void InitGame()
        {
            //Mientras doingSetup sea verdadero, el jugador no puede moverse, evita que el jugador se mueva mientras la tarjeta de título está arriba.
            doingSetup = true;

            //Obtiene una referencia a nuestra imagen LevelImage encontrándola por nombre.
            levelImage = GameObject.Find("LevelImage");

            //Obtiene una referencia al componente de texto de LevelText encontrándolo por nombre y llamando a GetComponent.
            levelText = GameObject.Find("LevelText").GetComponent<Text>();

            //Establece el texto de levelText a la cadena "Día" y agrega el número del nivel actual.
            levelText.text = "Día " + level;

            //Activa levelImage para bloquear la vista del jugador del tablero del juego durante la configuración.
            levelImage.SetActive(true);

            //Llama a la función HideLevelImage con un retraso en segundos de levelStartDelay.
            Invoke("HideLevelImage", levelStartDelay);

            //Limpia cualquier objeto Enemy en nuestra lista para preparar el próximo nivel.
            enemies.Clear();

            //Llama a la función SetupScene del script BoardManager, pasa el número del nivel actual.
            boardScript.SetupScene(level);
        }

        //Oculta la imagen negra utilizada entre niveles
        void HideLevelImage()
        {
            //Desactiva el objeto gameObject levelImage.
            levelImage.SetActive(false);

            //Establece doingSetup a false, permitiendo que el jugador se mueva nuevamente.
            doingSetup = false;
        }

        //Update se llama en cada cuadro.
        void Update()
        {
            //Verifica que playersTurn o enemiesMoving o doingSetup no sean verdaderos actualmente.
            if (playersTurn || enemiesMoving || doingSetup)
                //Si alguno de estos es verdadero, regresa y no inicia MoveEnemies.
                return;

            //Comienza a mover a los enemigos.
            StartCoroutine(MoveEnemies());
        }

        //Llama a esto para agregar el Enemy pasado a la lista de objetos Enemy.
        public void AddEnemyToList(Enemy script)
        {
            //Agrega Enemy a la lista enemies.
            enemies.Add(script);
        }

        //GameOver se llama cuando el jugador llega a 0 puntos de comida
        public void GameOver()
        {
            //Establece levelText para mostrar el número de niveles pasados y el mensaje de fin de juego
            levelText.text = "Después de " + level + " días,\n te moriste de hambre.";

            //Activa el objeto gameObject de fondo negro levelImage.
            levelImage.SetActive(true);

            //Desactiva este GameManager.
            enabled = false;
        }

        //Corrutina para mover a los enemigos en secuencia.
        IEnumerator MoveEnemies()
        {
            //Mientras enemiesMoving sea verdadero, el jugador no puede moverse.
            enemiesMoving = true;

            //Espera durante turnDelay segundos, por defecto .1 (100 ms).
            yield return new WaitForSeconds(turnDelay);

            //Si no hay enemigos generados (es decir, en el primer nivel):
            if (enemies.Count == 0)
            {
                //Espera durante turnDelay segundos entre movimientos, reemplaza el retraso causado por el movimiento de los enemigos cuando no hay ninguno.
                yield return new WaitForSeconds(turnDelay);
            }

            //Recorre la lista de objetos Enemy.
            for (int i = 0; i < enemies.Count; i++)
            {
                //Llama a la función MoveEnemy de Enemy en el índice i en la lista enemies.
                enemies[i].MoveEnemy();

                //Espera durante moveTime de Enemy antes de mover al siguiente Enemy.
                yield return new WaitForSeconds(enemies[i].moveTime);
            }

            //Una vez que los enemigos han terminado de moverse, establece playersTurn a true para que el jugador pueda moverse.
            playersTurn = true;

            //Los enemigos han terminado de moverse, establece enemiesMoving a false.
            enemiesMoving = false;
        }
    }
}
