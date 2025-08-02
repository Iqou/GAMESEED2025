using UnityEngine;
using System.Collections; // Diperlukan untuk Coroutine

public class Spawner : MonoBehaviour
{
    public GameObject objectToSpawn; // Prefab dari objek yang akan di-spawn (misal: panah biru)
    public float minSpawnDelay = 1f; // Waktu minimum antara spawn
    public float maxSpawnDelay = 3f; // Waktu maksimum antara spawn

    // **** PENTING: Sesuaikan nilai ini di Inspector ****
    // Ini harus berada di atas area terlihat layar bagian bawah DAN di atas border MissedClick
    public float spawnYPosition = -6.1f; // Posisi Y tetap untuk spawn (sesuaikan dengan game Anda)

    public float spawnZPosition = 0f; // Posisi Z tetap untuk spawn

    void Start()
    {
        // Mulai coroutine untuk melakukan spawning terus-menerus
        StartCoroutine(SpawnObjects());
    }

    IEnumerator SpawnObjects()
    {
        while (true) // Loop tak terbatas untuk spawning terus-menerus
        {
            float spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(spawnDelay); // Tunggu selama delay

            // Tentukan posisi spawn
            Vector3 spawnPosition = new Vector3(
                this.transform.position.x, // Menggunakan posisi X dari objek Spawner
                spawnYPosition,           // Menggunakan Y yang diatur di Inspector
                spawnZPosition            // Menggunakan Z yang diatur di Inspector
            );

            // Instansiasi objek
            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

            // Jika objectToSpawn memiliki script MoveSpawn dan Rigidbody2D,
            // maka script MoveSpawn.Start() akan berjalan otomatis.
        }
    }
}