using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class Timer : MonoBehaviour
    {
        public float timeRemaining = 60f; // Tiempo inicial del temporizador en segundos.
        public Text timerText;            // Referencia al componente de texto del temporizador.

        private bool timerIsRunning = false;

        void Start()
        {
            // Iniciar el temporizador.
            timerIsRunning = true;
        }

        void Update()
        {
            if (timerIsRunning)
            {
                if (timeRemaining > 0)
                {
                    timeRemaining -= Time.deltaTime;
                    DisplayTime(timeRemaining);
                }
                else
                {
                    Debug.Log("Time has run out!");
                    timeRemaining = 0;
                    timerIsRunning = false;
                    // Llama a la funci�n GameOver del GameManager.
                    GameManager.instance.GameOver();
                }
            }
        }

        // Funci�n para mostrar el tiempo restante en el formato mm:ss.
        void DisplayTime(float timeToDisplay)
        {
            timeToDisplay += 1;

            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // Funci�n para reiniciar el temporizador.
        public void ResetTimer()
        {
            timeRemaining = 60f;
            timerIsRunning = true;
        }
    }
}

