using UnityEngine;
using System.Collections;

namespace Completed
{
    //Enemy hereda de MovingObject, nuestra clase base para objetos que pueden moverse. Player también hereda de esta.
    public class Enemy : MovingObject
    {
        public int playerDamage;                            //La cantidad de puntos de comida a restar al jugador cuando ataque.
        public AudioClip attackSound1;                      //El primero de dos clips de audio para reproducir al atacar al jugador.
        public AudioClip attackSound2;                      //El segundo de dos clips de audio para reproducir al atacar al jugador.


        private Animator animator;                          //Variable de tipo Animator para almacenar una referencia al componente Animator del enemigo.
        private Transform target;                           //Transform para intentar moverse hacia cada turno.
        private bool skipMove;                              //Booleano para determinar si el enemigo debe saltar un turno o moverse en este turno.


        //Start anula la función Start virtual de la clase base.
        protected override void Start()
        {
            //Registrar este enemigo con nuestra instancia de GameManager añadiéndolo a una lista de objetos Enemy. 
            //Esto permite al GameManager emitir comandos de movimiento.
            GameManager.instance.AddEnemyToList(this);

            //Obtener y almacenar una referencia al componente Animator adjunto.
            animator = GetComponent<Animator>();

            //Encontrar el GameObject del jugador usando su etiqueta y almacenar una referencia a su componente transform.
            target = GameObject.FindGameObjectWithTag("Player").transform;

            //Llamar a la función start de nuestra clase base MovingObject.
            base.Start();
        }


        //Anula la función AttemptMove de MovingObject para incluir la funcionalidad necesaria para que Enemy salte turnos.
        //Ver comentarios en MovingObject para más detalles sobre cómo funciona la función base AttemptMove.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            //Verificar si skipMove es verdadero, si es así, establecerlo en falso y saltar este turno.
            if (skipMove)
            {
                skipMove = false;
                return;

            }

            //Llamar a la función AttemptMove de MovingObject.
            base.AttemptMove<T>(xDir, yDir);

            //Ahora que Enemy se ha movido, establecer skipMove en verdadero para saltar el siguiente movimiento.
            skipMove = true;
        }


        //MoveEnemy es llamado por GameManager cada turno para decirle a cada Enemy que intente moverse hacia el jugador.
        public void MoveEnemy()
        {
            //Declarar variables para las direcciones de movimiento en los ejes X e Y, estos varían de -1 a 1.
            //Estos valores nos permiten elegir entre las direcciones cardinales: arriba, abajo, izquierda y derecha.
            int xDir = 0;
            int yDir = 0;

            //Si la diferencia en posiciones es aproximadamente cero (Epsilon), hacer lo siguiente:
            if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)

                //Si la coordenada y de la posición del objetivo (jugador) es mayor que la coordenada y de la posición de este enemigo, establecer la dirección y en 1 (para moverse hacia arriba). Si no, establecerla en -1 (para moverse hacia abajo).
                yDir = target.position.y > transform.position.y ? 1 : -1;

            //Si la diferencia en posiciones no es aproximadamente cero (Epsilon), hacer lo siguiente:
            else
                //Verificar si la posición x del objetivo es mayor que la posición x del enemigo, si es así, establecer la dirección x en 1 (moverse a la derecha), si no, establecerla en -1 (moverse a la izquierda).
                xDir = target.position.x > transform.position.x ? 1 : -1;

            //Llamar a la función AttemptMove y pasar el parámetro genérico Player, porque Enemy se está moviendo y espera potencialmente encontrarse con un Player.
            AttemptMove<Player>(xDir, yDir);
        }


        //OnCantMove es llamado si Enemy intenta moverse a un espacio ocupado por un Player. Anula la función OnCantMove de MovingObject 
        //y toma un parámetro genérico T que usamos para pasar el componente que esperamos encontrar, en este caso Player.
        protected override void OnCantMove<T>(T component)
        {
            //Declarar hitPlayer y establecerlo igual al componente encontrado.
            Player hitPlayer = component as Player;

            //Llamar a la función LoseFood de hitPlayer pasándole playerDamage, la cantidad de puntos de comida a restar.
            hitPlayer.LoseFood(playerDamage);

            //Establecer el trigger de ataque del animator para activar la animación de ataque de Enemy.
            animator.SetTrigger("enemyAttack");

            //Llamar a la función RandomizeSfx de SoundManager pasándole los dos clips de audio para elegir aleatoriamente entre ellos.
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }
    }
}
