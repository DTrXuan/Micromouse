using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MouseController : MonoBehaviour
{
	public bool connectToSerial;

	void Start()
	{
		GetComponentInChildren<MouseSerialListener>().enabled = connectToSerial;
		GetComponentInChildren<SerialController>().enabled = connectToSerial;
	}

	[CustomEditor(typeof(MouseController))]
	private class MouseControllerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var mouse = (MouseController) target;

			GUI.enabled = !Application.isPlaying;
			mouse.connectToSerial = EditorGUILayout.Toggle(new GUIContent("Connect to Serial"), mouse.connectToSerial);
		}
	}
}
