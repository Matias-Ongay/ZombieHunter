using UnityEngine;
using System.Collections;

namespace Completed
{
    public class SoundManager : MonoBehaviour
    {
        public AudioSource efxSource; // Arrastra una referencia al audio source que reproducirá los efectos de sonido.
        public AudioSource musicSource; // Arrastra una referencia al audio source que reproducirá la música.
        public static SoundManager instance = null; // Permite que otros scripts llamen a funciones de SoundManager.
        public float lowPitchRange = .95f; // El tono más bajo que se reproducirá de un efecto de sonido de forma aleatoria.
        public float highPitchRange = 1.05f; // El tono más alto que se reproducirá de un efecto de sonido de forma aleatoria.

        void Awake()
        {
            // Comprueba si ya existe una instancia de SoundManager
            if (instance == null)
                // Si no, establece esta como la instancia.
                instance = this;
            // Si ya existe una instancia:
            else if (instance != this)
                // Destruye esta, esto impone nuestro patrón singleton para que solo pueda haber una instancia de SoundManager.
                Destroy(gameObject);

            // Establece SoundManager para que no se destruya al cargar otra escena.
            DontDestroyOnLoad(gameObject);
        }

        // Se usa para reproducir clips de sonido individuales.
        public void PlaySingle(AudioClip clip)
        {
            // Establece el clip de nuestro audio source efxSource al clip pasado como parámetro.
            efxSource.clip = clip;

            // Reproduce el clip.
            efxSource.Play();
        }

        // RandomizeSfx elige aleatoriamente entre varios clips de audio y cambia ligeramente su tono.
        public void RandomizeSfx(params AudioClip[] clips)
        {
            // Genera un número aleatorio entre 0 y la longitud de nuestro array de clips pasados.
            int randomIndex = Random.Range(0, clips.Length);

            // Elige un tono aleatorio para reproducir nuestro clip entre nuestros rangos de tono alto y bajo.
            float randomPitch = Random.Range(lowPitchRange, highPitchRange);

            // Establece el tono del audio source al tono elegido aleatoriamente.
            efxSource.pitch = randomPitch;

            // Establece el clip al clip en nuestro índice elegido aleatoriamente.
            efxSource.clip = clips[randomIndex];

            // Reproduce el clip.
            efxSource.Play();
        }
    }
}
