using UnityEngine;
using System.Collections;

namespace Completed
{
    public class Wall : MonoBehaviour
    {
        public AudioClip chopSound1; // 1 de 2 clips de audio que se reproducen cuando el jugador ataca la pared.
        public AudioClip chopSound2; // 2 de 2 clips de audio que se reproducen cuando el jugador ataca la pared.
        public Sprite dmgSprite; // Sprite alternativo para mostrar después de que el jugador haya atacado la pared.
        public int hp = 3; // Puntos de vida para la pared.

        private SpriteRenderer spriteRenderer; // Almacena una referencia del componente SpriteRenderer adjunto.

        void Awake()
        {
            // Obtén una referencia del componente SpriteRenderer.
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // DamageWall se llama cuando el jugador ataca una pared.
        public void DamageWall(int loss)
        {
            // Llama a la función RandomizeSfx de SoundManager para reproducir uno de los dos sonidos de corte.
            SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

            // Establece spriteRenderer al sprite de la pared dañada.
            spriteRenderer.sprite = dmgSprite;

            // Resta loss del total de puntos de vida.
            hp -= loss;

            // Si los puntos de vida son menores o iguales a cero:
            if (hp <= 0)
                // Desactiva el gameObject.
                gameObject.SetActive(false);
        }
    }
}
