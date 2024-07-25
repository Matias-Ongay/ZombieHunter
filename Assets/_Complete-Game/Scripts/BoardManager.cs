using UnityEngine;
using System;
using System.Collections.Generic; 		//Nos permite usar Listas.
using Random = UnityEngine.Random; 		//Indica a Random que use el generador de números aleatorios del motor de Unity.

namespace Completed

{

    public class BoardManager : MonoBehaviour
    {
        // Usar Serializable nos permite incrustar una clase con subpropiedades en el inspector.
        [Serializable]
        public class Count
        {
            public int minimum;             //Valor mínimo para nuestra clase Count.
            public int maximum;             //Valor máximo para nuestra clase Count.


            //Constructor de asignación.
            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }


        public int columns = 8;                                         //Número de columnas en nuestro tablero de juego.
        public int rows = 8;                                            //Número de filas en nuestro tablero de juego.
        public Count wallCount = new Count(5, 9);                       //Límite inferior y superior para nuestro número aleatorio de muros por nivel.
        public Count foodCount = new Count(1, 5);                       //Límite inferior y superior para nuestro número aleatorio de alimentos por nivel.
        public GameObject exit;                                         //Prefab para generar la salida.
        public GameObject[] floorTiles;                                 //Arreglo de prefabs de suelos.
        public GameObject[] wallTiles;                                  //Arreglo de prefabs de muros.
        public GameObject[] foodTiles;                                  //Arreglo de prefabs de alimentos.
        public GameObject[] enemyTiles;                                 //Arreglo de prefabs de enemigos.
        public GameObject[] outerWallTiles;                             //Arreglo de prefabs de muros exteriores.

        private Transform boardHolder;                                  //Variable para almacenar una referencia al transform de nuestro objeto Board.
        private List<Vector3> gridPositions = new List<Vector3>();  //Lista de posibles ubicaciones para colocar las fichas.


        //Limpia nuestra lista gridPositions y la prepara para generar un nuevo tablero.
        void InitialiseList()
        {
            //Limpia nuestra lista gridPositions.
            gridPositions.Clear();

            //Recorre el eje x (columnas).
            for (int x = 1; x < columns - 1; x++)
            {
                //Dentro de cada columna, recorre el eje y (filas).
                for (int y = 1; y < rows - 1; y++)
                {
                    //En cada índice agrega un nuevo Vector3 a nuestra lista con las coordenadas x e y de esa posición.
                    gridPositions.Add(new Vector3(x, y, 0f));
                }
            }
        }


        //Configura los muros exteriores y el suelo (fondo) del tablero de juego.
        void BoardSetup()
        {
            //Instancia el tablero y asigna boardHolder a su transform.
            boardHolder = new GameObject("Board").transform;

            //Recorre el eje x, comenzando desde -1 (para llenar la esquina) con suelos o fichas de borde de muros exteriores.
            for (int x = -1; x < columns + 1; x++)
            {
                //Recorre el eje y, comenzando desde -1 para colocar fichas de suelos o muros exteriores.
                for (int y = -1; y < rows + 1; y++)
                {
                    //Elige una ficha aleatoria de nuestro arreglo de prefabs de suelos y prepárate para instanciarla.
                    GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                    //Verifica si la posición actual está en el borde del tablero, si es así elige un prefab de muro exterior aleatorio de nuestro arreglo de muros exteriores.
                    if (x == -1 || x == columns || y == -1 || y == rows)
                        toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

                    //Instancia el GameObject usando el prefab elegido para toInstantiate en el Vector3 correspondiente a la posición de la cuadrícula actual en el bucle, convertida a GameObject.
                    GameObject instance =
                        Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                    //Asigna el padre de nuestro objeto recién instanciado a boardHolder, esto es solo para organización y evitar desorden en la jerarquía.
                    instance.transform.SetParent(boardHolder);
                }
            }
        }


        //RandomPosition devuelve una posición aleatoria de nuestra lista gridPositions.
        Vector3 RandomPosition()
        {
            //Declara un entero randomIndex, asigna su valor a un número aleatorio entre 0 y el número de elementos en nuestra lista gridPositions.
            int randomIndex = Random.Range(0, gridPositions.Count);

            //Declara una variable de tipo Vector3 llamada randomPosition, asigna su valor a la entrada en randomIndex de nuestra lista gridPositions.
            Vector3 randomPosition = gridPositions[randomIndex];

            //Elimina la entrada en randomIndex de la lista para que no pueda volver a usarse.
            gridPositions.RemoveAt(randomIndex);

            //Devuelve la posición Vector3 seleccionada aleatoriamente.
            return randomPosition;
        }


        //LayoutObjectAtRandom acepta un arreglo de objetos de juego para elegir junto con un rango mínimo y máximo para el número de objetos a crear.
        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            //Elige un número aleatorio de objetos para instanciar dentro de los límites mínimo y máximo.
            int objectCount = Random.Range(minimum, maximum + 1);

            //Instancia objetos hasta que se alcance el límite elegido aleatoriamente objectCount.
            for (int i = 0; i < objectCount; i++)
            {
                //Elige una posición para randomPosition obteniendo una posición aleatoria de nuestra lista de Vector3 disponibles almacenados en gridPosition.
                Vector3 randomPosition = RandomPosition();

                //Elige una ficha aleatoria de tileArray y asígnala a tileChoice.
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

                //Instancia tileChoice en la posición devuelta por RandomPosition sin cambiar la rotación.
                Instantiate(tileChoice, randomPosition, Quaternion.identity);
            }
        }


        //SetupScene inicializa nuestro nivel y llama a las funciones anteriores para disponer el tablero de juego.
        public void SetupScene(int level)
        {
            //Crea los muros exteriores y el suelo.
            BoardSetup();

            //Reinicia nuestra lista de posiciones de la cuadrícula.
            InitialiseList();

            //Instancia un número aleatorio de fichas de muros basado en el mínimo y máximo, en posiciones aleatorias.
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

            //Instancia un número aleatorio de fichas de alimentos basado en el mínimo y máximo, en posiciones aleatorias.
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

            //Determina el número de enemigos basado en el número de nivel actual, basado en una progresión logarítmica.
            int enemyCount = (int)Mathf.Log(level, 2f);

            //Instancia un número aleatorio de enemigos basado en el mínimo y máximo, en posiciones aleatorias.
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

            //Instancia la ficha de salida en la esquina superior derecha de nuestro tablero de juego.
            Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
        }
    }
}
