using UnityEngine;
using System.Collections;

namespace Completed
{
    public class Loader : MonoBehaviour
    {
        public GameObject gameManager;          //Prefab de GameManager para instanciar.
        public GameObject soundManager;         //Prefab de SoundManager para instanciar.


        void Awake()
        {
            //Comprueba si un GameManager ya ha sido asignado a la variable estática GameManager.instance o si aún está nulo
            if (GameManager.instance == null)
                //Instancia el prefab de gameManager
                Instantiate(gameManager);

            //Comprueba si un SoundManager ya ha sido asignado a la variable estática SoundManager.instance o si aún está nulo
            if (SoundManager.instance == null)
                //Instancia el prefab de soundManager
                Instantiate(soundManager);
        }
    }
}
