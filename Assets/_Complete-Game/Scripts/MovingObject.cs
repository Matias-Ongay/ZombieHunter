using UnityEngine;
using System.Collections;

namespace Completed
{
    //La palabra clave abstract permite crear clases y miembros de clase que están incompletos y deben ser implementados en una clase derivada.
    public abstract class MovingObject : MonoBehaviour
    {
        public float moveTime = 1f;         //Tiempo que tomará el objeto para moverse, en segundos.
        public LayerMask blockingLayer;     //Capa en la que se comprobará la colisión.

        private BoxCollider2D boxCollider;  //El componente BoxCollider2D adjunto a este objeto.
        private Rigidbody2D rb2D;           //El componente Rigidbody2D adjunto a este objeto.
        private float inverseMoveTime;      //Usado para hacer el movimiento más eficiente.
        private bool isMoving;              //Indica si el objeto se está moviendo actualmente.

        //Las funciones protegidas y virtuales pueden ser sobrescritas por las clases que heredan.
        protected virtual void Start()
        {
            //Obtiene una referencia al componente BoxCollider2D de este objeto.
            boxCollider = GetComponent<BoxCollider2D>();

            //Obtiene una referencia al componente Rigidbody2D de este objeto.
            rb2D = GetComponent<Rigidbody2D>();

            //Al almacenar el recíproco del tiempo de movimiento podemos usarlo multiplicando en lugar de dividiendo, esto es más eficiente.
            inverseMoveTime = 1f / moveTime;
        }

        //Move devuelve true si puede moverse y false si no. 
        //Move toma parámetros para la dirección en x, la dirección en y y un RaycastHit2D para comprobar la colisión.
        protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
        {
            //Almacena la posición inicial desde la cual moverse, basada en la posición actual del transform del objeto.
            Vector2 start = transform.position;

            //Calcula la posición final basada en los parámetros de dirección pasados al llamar a Move.
            Vector2 end = start + new Vector2(xDir, yDir);

            //Desactiva el boxCollider para que el linecast no golpee el propio collider de este objeto.
            boxCollider.enabled = false;

            //Lanza una línea desde el punto de inicio al punto final comprobando colisión en blockingLayer.
            hit = Physics2D.Linecast(start, end, blockingLayer);

            //Vuelve a habilitar el boxCollider después del linecast.
            boxCollider.enabled = true;

            //Comprueba si no se golpeó nada y el objeto no se está moviendo.
            if (hit.transform == null && !isMoving)
            {
                //Inicia la corrutina SmoothMovement pasando el Vector2 end como destino.
                StartCoroutine(SmoothMovement(end));

                //Devuelve true para indicar que Move fue exitoso.
                return true;
            }

            //Si se golpeó algo, devuelve false, Move fue fallido.
            return false;
        }

        //Corrutina para mover unidades de un espacio al siguiente, toma un parámetro end para especificar a dónde moverse.
        protected IEnumerator SmoothMovement(Vector3 end)
        {
            //El objeto se está moviendo ahora.
            isMoving = true;

            //Calcula la distancia restante para moverse basada en la magnitud cuadrada de la diferencia entre la posición actual y el parámetro end. 
            //Se usa magnitud cuadrada en lugar de magnitud porque es computacionalmente más barato.
            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            //Mientras esa distancia sea mayor que una cantidad muy pequeña (Epsilon, casi cero):
            while (sqrRemainingDistance > float.Epsilon)
            {
                //Encuentra una nueva posición proporcionalmente más cercana al final, basada en el tiempo de movimiento.
                Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

                //Llama a MovePosition en el Rigidbody2D adjunto y lo mueve a la posición calculada.
                rb2D.MovePosition(newPosition);

                //Recalcula la distancia restante después de moverse.
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;

                //Devuelve y repite hasta que sqrRemainingDistance esté lo suficientemente cerca de cero para finalizar la función.
                yield return null;
            }

            //Asegura que el objeto esté exactamente al final de su movimiento.
            rb2D.MovePosition(end);

            //El objeto ya no se está moviendo.
            isMoving = false;
        }

        //La palabra clave virtual significa que AttemptMove puede ser sobrescrita por las clases que heredan usando la palabra clave override.
        //AttemptMove toma un parámetro genérico T para especificar el tipo de componente con el que esperamos que nuestra unidad interactúe si está bloqueado (Player para Enemies, Wall para Player).
        protected virtual void AttemptMove<T>(int xDir, int yDir)
            where T : Component
        {
            //Hit almacenará lo que sea que nuestro linecast golpee cuando se llame a Move.
            RaycastHit2D hit;

            //Establece canMove a true si Move fue exitoso, false si falló.
            bool canMove = Move(xDir, yDir, out hit);

            //Comprueba si el linecast no golpeó nada.
            if (hit.transform == null)
                //Si no golpeó nada, devuelve y no ejecuta más código.
                return;

            //Obtiene una referencia al componente de tipo T adjunto al objeto que fue golpeado.
            T hitComponent = hit.transform.GetComponent<T>();

            //Si canMove es false y hitComponent no es igual a null, significa que MovingObject está bloqueado y ha golpeado algo con lo que puede interactuar.
            if (!canMove && hitComponent != null)
                //Llama a la función OnCantMove y le pasa hitComponent como parámetro.
                OnCantMove(hitComponent);
        }

        //El modificador abstract indica que lo que se está modificando tiene una implementación faltante o incompleta.
        //OnCantMove será sobrescrita por funciones en las clases que heredan.
        protected abstract void OnCantMove<T>(T component)
            where T : Component;
    }
}
