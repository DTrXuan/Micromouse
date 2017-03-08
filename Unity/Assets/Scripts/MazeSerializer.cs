using System.IO;
using UnityEditor;
using UnityEngine;

public class MazeSerializer : MonoBehaviour
{
	public static string MazeName { get; private set; }

	public static void ResetMazeName()
	{
		MazeName = "Custom";
	}

	public static void LoadFromFile()
	{
		var path = EditorUtility.OpenFilePanelWithFilters("Load maze...", "Assets/Mazes/", new []{"Maze", "maz"});

		if (path.Length == 0)
			return;

		var data = File.ReadAllBytes(path);

		Maze.Cells = new Maze.Cell[16, 16];

		int i = 0;
		for (int x = 0; x < Maze.MazeSize.x; x++)
		{
			for (int y = 0; y < Maze.MazeSize.y; y++)
			{
				Maze.Cells[x, y] = new Maze.Cell(x, y, (Maze.Wall) data[i]);
				i++;
			}
		}
		
		MazeName = Path.GetFileNameWithoutExtension(path);
		FindObjectOfType<MazeBuilder>().Render();
	}

	public static void SaveToFile()
	{
		var path = EditorUtility.SaveFilePanelInProject("Save maze as...", "", "maz", "Assets/Mazes/");
		var data = new byte[Maze.MazeSize.x * Maze.MazeSize.y];
		
		int i = 0;
		for (int x = 0; x < Maze.MazeSize.x; x++)
		{
			for (int y = 0; y < Maze.MazeSize.y; y++)
			{
				var walls = Maze.Cells[x, y].walls;
				int _int = (int) walls;
				byte b = (byte) _int;
				data[i] = b;
				i++;
			}
		}

		if (path.Length != 0)
		{
			File.WriteAllBytes(path, data);
			MazeName = Path.GetFileNameWithoutExtension(path);
		}
	}

	[CustomEditor(typeof(MazeSerializer))]
	private class MazeSerializerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (Application.isPlaying)
				EditorGUILayout.LabelField(MazeSerializer.MazeName);
			else
				EditorGUILayout.LabelField("");

			GUI.enabled = Application.isPlaying;

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Load"))
					MazeSerializer.LoadFromFile();

				if (GUILayout.Button("Save"))
					MazeSerializer.SaveToFile();
			}
			GUILayout.EndHorizontal();
		}
	}
}