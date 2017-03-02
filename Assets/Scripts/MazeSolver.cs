using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MazeSolver : MonoBehaviour
{
	public enum Algorithm
	{
		RightWall,
		FloodFill,
	}

	[Flags]
	public enum MouseOrientation
	{
		North = 1 << 0, // 1
		East = 1 << 1, // 2
		South = 1 << 2, // 4
		West = 1 << 3, // 8
	}

	public static MazeGenerator.Wall GetWallFromMouseOrientation(MouseOrientation mouseOrientation)
	{
		switch (mouseOrientation)
		{
			case MouseOrientation.North:
				return MazeGenerator.Wall.North;

			case MouseOrientation.East:
				return MazeGenerator.Wall.East;

			case MouseOrientation.South:
				return MazeGenerator.Wall.South;

			case MouseOrientation.West:
				return MazeGenerator.Wall.West;
		}

		return MazeGenerator.Wall.None;
	}

	public float stepTime;
	public Algorithm algorithm;
	public float sensorDetectionLength;

	private MouseOrientation orientation;
	private MazeGenerator.Cell[,] originalMaze;
	private MazeGenerator.Cell[,] cells;
	
	private bool pauseSolve;
	private bool pauseRun;
	private Algorithm algorithmInUse;
	private int x, y;
	
	private Vector2 mazeSize, endSize;
	private Vector3 wallDimensions;

	private bool forceMove;

	private void Start()
	{
		Reset();
	}

	private void Reset()
	{
		var mazeBuilder = FindObjectOfType<MazeBuilder>();
		originalMaze = mazeBuilder.GetMaze();

		mazeSize = mazeBuilder.MazeSize;
		endSize = mazeBuilder.EndSize;
		wallDimensions = mazeBuilder.WallDimensions;

		cells = new MazeGenerator.Cell[(int) mazeSize.x, (int) mazeSize.y];

		MazeGenerator.Cell start = originalMaze[0, 0];
		orientation = MouseOrientation.North;
		if (start.HasWall(MazeGenerator.Wall.North))
			orientation = MouseOrientation.East;
		
		GameObject.FindGameObjectsWithTag("Wall").ToList().ForEach(go => go.GetComponent<Renderer>().material.color = Color.white);
		GameObject.FindGameObjectsWithTag("Ground").ToList().ForEach(go => go.GetComponent<Renderer>().material.color = Color.black);

		x = 0;
		y = 0;

		UpdateTransform();
	}

	public void Solve()
	{
		Reset();

		algorithmInUse = algorithm;
		pauseSolve = false;
		StartCoroutine(StepSolve());
	}

	private IEnumerator StepSolve()
	{
		yield return new WaitWhile(() => pauseSolve);

		TouchCell();

		switch (algorithmInUse)
		{
			case Algorithm.RightWall:
				RightWallStep();
				break;

			case Algorithm.FloodFill:
				FloodFillStep();
				break;
		}

		UpdateTransform();

		yield return new WaitForSeconds(stepTime);
		yield return StartCoroutine(StepSolve());
	}
	
	private void StopSolve()
	{
		StopAllCoroutines();
	}

	// Negative CCW; Positive CW
	private void Rotate(int quarters)
	{
		orientation = GetOrientation(orientation, quarters);
	}

	private MouseOrientation GetOrientation(MouseOrientation orientation, int quarters)
	{
		var mouseOrientationLength = Enum.GetNames(typeof(MouseOrientation)).Length;

		if (quarters < 0)
			quarters += mouseOrientationLength;

		int index = (Array.IndexOf(Enum.GetValues(typeof(MouseOrientation)), orientation) + quarters) % mouseOrientationLength;
		return (MouseOrientation) (Enum.GetValues(orientation.GetType())).GetValue(index);
	}

	private void RightWallStep()
	{
		var front = GetWallFromMouseOrientation(orientation);
		var right = GetWallFromMouseOrientation(GetOrientation(orientation, 1));

		var hasFrontWall = cells[x, y].HasWall(front);
		var hasRightWall = cells[x, y].HasWall(right);

		if (forceMove)
		{
			MoveForward();
			forceMove = false;
			return;
		}

		if (!hasRightWall)
		{
			Rotate(1);
			forceMove = true;
		}
		else if (!hasFrontWall)
		{
			MoveForward();
		}
		else
		{
			Rotate(-1);
		}
	}

	private void FloodFillStep()
	{
	}

	private void MoveForward(int length = 1)
	{
		if ((orientation & MouseOrientation.North) != 0)
		{
			y += length;
		}
		else if ((orientation & MouseOrientation.South) != 0)
		{
			y -= length;
		}

		if ((orientation & MouseOrientation.East) != 0)
		{
			x += length;
		}
		else if ((orientation & MouseOrientation.West) != 0)
		{
			x -= length;
		}
	}

	private void UpdateTransform()
	{
		var wallDelta = wallDimensions.x - wallDimensions.z;

		Vector3 position = Vector3.zero;
		position += new Vector3(0, 0, wallDelta);
		position += Vector3.up * wallDimensions.y;

		position += Vector3.forward * y * wallDelta;
		position += Vector3.right * x * wallDelta;
		//position += Vector3.up * 0.03f;

		int index = Array.IndexOf(Enum.GetValues(typeof(MouseOrientation)), orientation);
		var rotation = Quaternion.Euler(0, 90 * (int) index, 0);

		this.transform.rotation = rotation;
		this.transform.position = position;
	}

	private void TouchCell()
	{
		if (cells[x, y] == null)
		{
			cells[x, y] = new MazeGenerator.Cell(x, y);
		}

		cells[x, y].visited = true;

		var front = orientation;
		var left = GetOrientation(orientation, -1);
		var right = GetOrientation(orientation, 1);
		
		DetectGround();
		DetectWall(front);
		DetectWall(left);
		DetectWall(right);
	}
	
	private void DetectGround()
	{
		var ray = new Ray(transform.position, -transform.up);
		var hitInfo = new RaycastHit();
		if (Physics.Raycast(ray, out hitInfo))
		{
			if (hitInfo.transform.tag == "Ground")
				hitInfo.transform.GetComponent<Renderer>().material.color = Color.Lerp(Color.black, Color.red, 0.2f);
			else if (hitInfo.transform.tag == "End")
				hitInfo.transform.GetComponent<Renderer>().material.color = Color.Lerp(Color.black, Color.red, 0.5f);
		}
	}

	private void DetectWall(MouseOrientation orientation)
	{
		var wall = GetWallFromMouseOrientation(orientation);

		if (originalMaze[x, y].HasWall(wall))
		{
			cells[x, y].InsertWall(wall);

			var ray = new Ray(transform.position, GetOrientationVector(orientation));
			var hitInfo = new RaycastHit();
			if (Physics.Raycast(ray, out hitInfo))
			{
				if (hitInfo.transform.tag == "Wall" && hitInfo.distance < sensorDetectionLength)
					hitInfo.transform.GetComponent<Renderer>().material.color = Color.green;
			}
		}
	}

	private Vector3 GetOrientationVector(MouseOrientation orientation)
	{
		int x = 0;
		int y = 0;

		if ((orientation & MouseOrientation.North) != 0)
		{
			y = 1;
		}
		else if ((orientation & MouseOrientation.South) != 0)
		{
			y = -1;
		}

		if ((orientation & MouseOrientation.East) != 0)
		{
			x = 1;
		}
		else if ((orientation & MouseOrientation.West) != 0)
		{
			x = -1;
		}

		return new Vector3(x, 0, y).normalized;
	}

	private IEnumerator StepRun()
	{
		// TODO: implement run
		yield break;
	}
	
	private void StopRun()
	{
		StopCoroutine(StepRun());	
	}

	[CustomEditor(typeof(MazeSolver))]
	private class MazeSolverEditor : Editor
	{
		private bool solving;
		private bool running;

		public override void OnInspectorGUI()
		{
			var solver = (MazeSolver) target;

			solver.stepTime = EditorGUILayout.Slider(new GUIContent("Step Time (s)"), solver.stepTime, 0.01f, 1f);

			GUI.enabled = !solving && !running;
			solver.algorithm = (Algorithm) EditorGUILayout.EnumPopup(new GUIContent("Algorithm"), solver.algorithm);

			solver.sensorDetectionLength = EditorGUILayout.Slider(new GUIContent("Sensor Detection (m)"), solver.sensorDetectionLength, 0.01f, 0.30f);

			if (!Application.isPlaying)
				return;

			GUI.enabled = !running;
			if (!solving)
			{
				if (GUILayout.Button("Solve"))
				{
					solver.pauseSolve = false;
					solver.Solve();
					solving = true;
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				{
					if (!solver.pauseSolve)
					{
						if (GUILayout.Button("Pause solve"))
							solver.pauseSolve = true;
					}
					else
					{
						if (GUILayout.Button("Resume solve"))
							solver.pauseSolve = false;
					}

					if (GUILayout.Button("Stop solve"))
					{
						solver.pauseSolve = true;
						solving = false;
						solver.StopSolve();
						solver.Reset();
					}
				}
				GUILayout.EndHorizontal();
			}

			GUI.enabled = !solving;
			if (!running)
			{
				if (GUILayout.Button("Run"))
				{
					solver.pauseRun = false;
					solver.StepRun();
					running = true;
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				{
					if (!solver.pauseRun)
					{
						if(GUILayout.Button("Pause run"))
							solver.pauseRun = true;
					}
					else 
					{
						if (GUILayout.Button("Resume run"))
						{
							solver.pauseRun = false;
						}
					}

					if (GUILayout.Button("Stop run"))
					{
						solver.pauseRun = true;
						running = false;
						solver.StopRun();
						solver.Reset();
					}
				}
				GUILayout.EndHorizontal();
			}
		}
	}
}