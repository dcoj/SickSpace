using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;

public class WebsocketClient : MonoBehaviour
{
    public string serverURL = "ws://localhost:8080";
    private WebSocket ws;
    public Transform target; // Drag your target GameObject here
    public Transform camera; // Drag your camera GameObject here

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target GameObject not assigned!");
            return;
        }

        ws = new WebSocket(serverURL);

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket connected!");
        };

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Received: " + e.Data);
            try
            {
                // Deserialize the JSON data
                var data = JsonConvert.DeserializeObject<XYZData>(e.Data);

                // Use the dispatcher to update the target position on the main thread
                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    UpdateTargetPosition(data.x, data.y, data.z);
                });

            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error parsing JSON: " + ex.Message);
            }
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket Error: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket closed with reason: " + e.Reason);
        };

        ws.ConnectAsync();
    }

    void Update()
    {
        // Keep the connection alive (WebSocket-Sharp requires this)
        if (ws != null && ws.IsAlive)
        {
            // Do something to keep the connection alive, even if just checking the state.
        }
    }

    private void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }

    void UpdateTargetPosition(float x, float y, float z)
    {
        if (target != null && camera != null)
        {
            Vector3 cameraPosition = camera.position; // Get the camera's position
            Vector3 receivedOffset = new Vector3(x, y, (z * (-1))); // Create a Vector3 from the received data

            // Option 1: Target position is the camera's position *minus* the received offset
            // This would position the target relative to the camera, offset by the received values.
            Vector3 targetPosition = cameraPosition + receivedOffset;

            // Option 2: Target position is the camera's position *plus* the received offset
            // This might be more intuitive, depending on your use case.
            // Vector3 targetPosition = cameraPosition + receivedOffset;

            // Option 3:  Received data is in world space, and you want to offset it *from* the camera
            // Vector3 targetPosition = new Vector3(x, y, z) - cameraPosition;

            // Assign the target position
            target.position = targetPosition;
        }
        else
        {
            Debug.LogWarning("Camera or Target not assigned!");
        }
    }

    [System.Serializable]
    public class XYZData
    {
        public float x;
        public float y;
        public float z;
    }
}
