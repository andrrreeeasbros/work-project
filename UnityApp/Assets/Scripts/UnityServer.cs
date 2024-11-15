using UnityEngine;
using System.IO;
using System.Net.Sockets;
using System.Threading; // Для работы с потоками

public class UnityServer : MonoBehaviour
{
    public Camera frontCamera;
    public Camera mainCamera;
    public GameObject leftWall;

    public Light mainLight;
    private TcpListener listener;
    private Thread listenerThread;

    void Start()
    {
        frontCamera = GameObject.FindGameObjectWithTag("FrontCam")?.GetComponent<Camera>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
        leftWall = GameObject.FindGameObjectWithTag("leftWall");
        mainLight = GameObject.FindGameObjectWithTag("sceneLight")?.GetComponent<Light>();
        listenerThread = new Thread(new ThreadStart(ListenForMessages));
        listenerThread.IsBackground = true; // Сделать поток фоновым
        listenerThread.Start();
        leftWall.gameObject.SetActive(false);
        mainLight.intensity = 5;
    }

    // Метод для прослушивания сообщений от клиента
    void ListenForMessages()
    {
        try
        {
            listener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), 5000);; // Ожидаем на порту 12345
            listener.Start();
            Debug.Log($"Server started and listening for connections...");

            while (true)
            {
                var client = listener.AcceptTcpClient();
                var reader = new StreamReader(client.GetStream());
                string message = reader.ReadLine();

                Debug.Log("Received message: " + message);  // Логируем сообщение, полученное от клиента

                // Обрабатываем сообщение
                if (!string.IsNullOrEmpty(message) && message.Contains("SwitchCamera"))
                {
                    int cameraIndex = int.Parse(message.Split(':')[1]);
                    Debug.Log("Switching to camera " + cameraIndex); // Логируем, на какую камеру переключаемся
                    SwitchCamera(cameraIndex);
                }
                client.Close(); // Закрытие соединения
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in server thread: " + ex.Message);
        }
    }


    // Метод для переключения между камерами
    void SwitchCamera(int cameraIndex)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            // Камера FrontCam
            if (cameraIndex == 1)
            {
                mainLight.intensity = 1;
                frontCamera.gameObject.SetActive(true);
                mainCamera.gameObject.SetActive(false);
                leftWall.gameObject.SetActive(true);
                Debug.Log("Switched to Front Camera");
            }

            // Камера MainCamera
            else if (cameraIndex == 2)
            {
                mainLight.intensity = 5;
                leftWall.gameObject.SetActive(false);
                frontCamera.gameObject.SetActive(false);
                mainCamera.gameObject.SetActive(true);
                Debug.Log("Switched to Main Camera");
            }
        });
    }


    // Закрытие слушателя при завершении работы игры
    void OnApplicationQuit()
    {
        if (listener != null)
        {
            listener.Stop();
        }
        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Abort();
        }
    }
}