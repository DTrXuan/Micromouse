using UnityEngine;
using System.Collections;
using System.Text;

public class MouseSerialListener : MonoBehaviour
{
    public SerialController serialController;

    // Initialization
    void Start()
    {
        serialController.SetTearDownFunction(Teardown);
    }

    // Executed each frame
    void Update()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------

        // If you press one of these keys send it to the serial device. A
        // sample serial device that accepts this input is given in the README.
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Sending lights OFF");
            serialController.SendSerialMessage("K");
        }

		if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Sending lights ON");
            serialController.SendSerialMessage("L");
        }
		
		if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Testing serial");
            serialController.SendSerialMessage("T");
        }

        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------

        string message = serialController.ReadSerialMessage();

        if (message == null)
            return;

        // Check if the message is plain data or a connect/disconnect event.
        if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
            Debug.Log("Connection established");
        else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
	        Debug.Log("Connection attempt failed or disconnection detected");
        else
	        OnMessageArrived(message);
    }

    // Tear-down function for the hardware at the other side of the COM port
    public void Teardown()
    {
        Debug.Log("Executing teardown");
        serialController.SendSerialMessage("K");
    }

	/**/
	// Invoked when a line of data is received from the serial device.
    void OnMessageArrived(string message)
    {
		var bytes = Encoding.ASCII.GetBytes(message);

	    Debug.Log("Received " + bytes.Length + " bytes.");
    }
}
