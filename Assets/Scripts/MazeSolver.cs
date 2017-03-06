using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;

public class MazeSolver : MonoBehaviour
{
	private class FloodCell
	{
		public int x;
		public int y;
		public bool visited;
		public int value;

		public FloodCell(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	private enum FloodStatus
	{
		None,
		Find,
		Return
	}
	
	private MazeGenerator.Cell previousCell;
	private FloodCell[,] floodFill;
	private FloodStatus floodStatus;

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
	public Algorithm algorithmInUse;
	private int x, y;
	
	private Vector2 mazeSize, endSize;
	private Vector3 wallDimensions, pillarDimensions;

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
		pillarDimensions = mazeBuilder.PillarDimensions;

		cells = new MazeGenerator.Cell[(int) mazeSize.x, (int) mazeSize.y];
		for(y = 0; y < (int) mazeSize.y; y++)
		{
			for (x = 0; x < (int) mazeSize.x; x++)
			{
				cells[x, y] = new MazeGenerator.Cell(x, y);
			}
		}

		MazeGenerator.Cell start = originalMaze[0, 0];
		orientation = MouseOrientation.North;
		if (start.HasWall(MazeGenerator.Wall.North))
			orientation = MouseOrientation.East;
		
		GameObject.FindGameObjectsWithTag("Wall").ToList().ForEach(go => go.GetComponent<Renderer>().material.color = Color.white);
		GameObject.FindGameObjectsWithTag("Ground").ToList().ForEach(go => go.GetComponent<Renderer>().material.color = Color.black);
		GameObject.FindGameObjectsWithTag("End").ToList().ForEach(go => go.GetComponent<Renderer>().material.color = Color.gray);

		x = 0;
		y = 0;

		UpdateTransform();
	}

	public void Solve()
	{
		Reset();

		algorithmInUse = algorithm;
		pauseSolve = false;

		switch (algorithmInUse)
		{
			case Algorithm.FloodFill:
				SetupFloodFill();
				break;
		}

		StartCoroutine(StepSolve());
	}

	private void SetupFloodFill()
	{
		int columns = (int) mazeSize.x; 
		int rows = (int) mazeSize.y; 
		floodFill = new FloodCell[columns, rows];
		floodStatus = FloodStatus.Find;
		
		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < columns; x++)
			{
				floodFill[y, x] = new FloodCell(x, y);
			}
		}
		
		previousCell = null;
	}

	private MazeGenerator.Coord[] GetEndCells()
	{
		int columns = (int) mazeSize.x; 
		int rows = (int) mazeSize.y; 
		int endColumns = (int) endSize.x; 
		int endRows = (int) endSize.y; 

		int x0 = columns / 2 - endColumns / 2;
		int y0 = rows / 2 - endRows / 2;

		var cells = new MazeGenerator.Coord[endColumns * endRows];

		// Add end cells to the stack
		int i = 0;

		for (int xAux = 0; xAux < endColumns; xAux++)
		{
			for (int yAux = 0; yAux < endRows; yAux++)
			{
				cells[i] = new MazeGenerator.Coord(x0 + xAux, y0 + yAux);
				i++;
			}
		}

		return cells;
	}

	private void CalculateFloodFill(params MazeGenerator.Coord[] targetCells)
	{
		var currentStack = new Queue<MazeGenerator.Coord>();
		var nextStack = new Queue<MazeGenerator.Coord>();
		int currentFloodValue = 0;
		
		int columns = (int) mazeSize.x; 
		int rows = (int) mazeSize.y;

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < columns; x++)
			{
				floodFill[y, x].visited = false;
			}
		}

		for (int i = 0; i < targetCells.Length; i++)
			nextStack.Enqueue(targetCells[i]);

		while (nextStack.Count > 0)
		{
			currentStack = new Queue<MazeGenerator.Coord>(nextStack);
			nextStack.Clear();

			int currentStackLength = currentStack.Count;
			while (currentStackLength > 0)
			{
				currentStackLength--;
				var coord = currentStack.Dequeue();
				floodFill[coord.x, coord.y].value = currentFloodValue;
				floodFill[coord.x, coord.y].visited = true;
				currentStack.Enqueue(coord);
			}

			while (currentStack.Count > 0)
			{
				var coord = currentStack.Dequeue();

				// left
				if (coord.x > 0 && !cells[coord.x, coord.y].HasWall(MazeGenerator.Wall.West) && !floodFill[coord.x - 1, coord.y].visited) 
					nextStack.Enqueue(new MazeGenerator.Coord(coord.x - 1, coord.y));
				
				// right
				if (coord.x < columns - 1 && !cells[coord.x, coord.y].HasWall(MazeGenerator.Wall.East) && !floodFill[coord.x + 1, coord.y].visited) 
					nextStack.Enqueue(new MazeGenerator.Coord(coord.x + 1, coord.y));

				// down
				if (coord.y > 0 && !cells[coord.x, coord.y].HasWall(MazeGenerator.Wall.South) && !floodFill[coord.x, coord.y-1].visited) 
					nextStack.Enqueue(new MazeGenerator.Coord(coord.x, coord.y-1));

				// up
				if (coord.y < rows - 1 && !cells[coord.x, coord.y].HasWall(MazeGenerator.Wall.North) && !floodFill[coord.x, coord.y+1].visited) 
					nextStack.Enqueue(new MazeGenerator.Coord(coord.x, coord.y+1));
			}

			currentFloodValue++;
		}
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
		var groundTag = DetectGround(paint: false);

		if (groundTag == "End")
			floodStatus = FloodStatus.Return;
		
		if (floodStatus == FloodStatus.Find)
			CalculateFloodFill(GetEndCells());
		else if (floodStatus == FloodStatus.Return)
			CalculateFloodFill(new MazeGenerator.Coord(0, 0));

		var front = orientation;
		var left = GetOrientation(orientation, -1);
		var right = GetOrientation(orientation, 1);
		
		int deltaRotation = 1;

		var hasFrontWall = cells[x, y].HasWall(GetWallFromMouseOrientation(front));
		var hasLeftWall = cells[x, y].HasWall(GetWallFromMouseOrientation(left));
		var hasRightWall = cells[x, y].HasWall(GetWallFromMouseOrientation(right));
		
		int lowestFlood = Int32.MaxValue;

		MazeGenerator.Cell cell = null;

		cell = GetAdjacentCell(front);
		if (!hasFrontWall && cell != null)
		{
			var adjacentFlood = floodFill[cell.x, cell.y].value;
			if (adjacentFlood < lowestFlood)
			{
				lowestFlood = adjacentFlood;
				deltaRotation = 0;
			}
		}
		
		cell = GetAdjacentCell(left);
		if (!hasLeftWall && cell != null)
		{
			var adjacentFlood = floodFill[cell.x, cell.y].value;
			if (adjacentFlood < lowestFlood)
			{
				lowestFlood = adjacentFlood;
				deltaRotation = -1;
			}
		}

		cell = GetAdjacentCell(right);
		if (!hasRightWall && cell != null)
		{
			var adjacentFlood = floodFill[cell.x, cell.y].value;
			if (adjacentFlood < lowestFlood)
			{
				lowestFlood = adjacentFlood;
				deltaRotation = 1;
			}
		}
		
		if (previousCell != null)
		{
			var adjacentFlood = floodFill[previousCell.x, previousCell.y].value;
			if (adjacentFlood < lowestFlood)
			{
				lowestFlood = adjacentFlood;
				deltaRotation = -2;
			}
		}

		previousCell = cells[x, y];

		Rotate(deltaRotation);
		MoveForward();
	}

	private MazeGenerator.Cell GetAdjacentCell(MouseOrientation orientation)
	{
		int xNew = x;
		int yNew = y;

		int length = 1;

		if ((orientation & MouseOrientation.North) != 0)
			yNew += 1;

		else if ((orientation & MouseOrientation.South) != 0)
			yNew -= length;

		if ((orientation & MouseOrientation.East) != 0)
			xNew += length;

		else if ((orientation & MouseOrientation.West) != 0)
			xNew -= length;

		if (xNew < 0 || xNew >= mazeSize.x)
			return null;
		
		if (yNew < 0 || yNew >= mazeSize.y)
			return null;

		return cells[xNew, yNew];
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
		var wallDelta = wallDimensions.x + pillarDimensions.x;

		Vector3 position = Vector3.zero;
		position += new Vector3(1, 0, 1) * wallDimensions.x / 2;
		position += new Vector3(1, 0, 1) * (pillarDimensions.x + pillarDimensions.z) / 2;
		position += Vector3.up * wallDimensions.y;
		position += Vector3.forward * y * wallDelta;
		position += Vector3.right * x * wallDelta;

		int index = Array.IndexOf(Enum.GetValues(typeof(MouseOrientation)), orientation);
		var rotation = Quaternion.Euler(0, 90 * (int) index, 0);

		this.transform.rotation = rotation;
		this.transform.position = position;
	}

	private void TouchCell()
	{
		cells[x, y].visited = true;

		var front = orientation;
		var left = GetOrientation(orientation, -1);
		var right = GetOrientation(orientation, 1);
		
		DetectGround(paint: true);
		DetectWall(front);
		DetectWall(left);
		DetectWall(right);
	}
	
	private String DetectGround(bool paint)
	{
		String groundTag = null;

		var ray = new Ray(transform.position, -transform.up);
		var hitInfo = new RaycastHit();
		if (Physics.Raycast(ray, out hitInfo))
		{
			if (hitInfo.transform.tag == "Ground")
			{
				if(paint) hitInfo.transform.GetComponent<Renderer>().material.color = Color.Lerp(Color.black, Color.red, 0.2f);
				groundTag = "Ground";
			}
			else if (hitInfo.transform.tag == "End")
			{
				if(paint) hitInfo.transform.GetComponent<Renderer>().material.color = Color.Lerp(Color.black, Color.red, 0.5f);
				groundTag = "End";
			}
		}
		
		return groundTag;
	}

	private void DetectWall(MouseOrientation orientation)
	{
		var wall = GetWallFromMouseOrientation(orientation);

		if (originalMaze[x, y].HasWall(wall))
		{
			cells[x, y].InsertWall(wall);

			var adjacentCell = GetAdjacentCell(orientation);
			if(adjacentCell != null) 
				cells[adjacentCell.x, adjacentCell.y].InsertWall(MazeGenerator.GetOppositeWall(wall));

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
			{
				if(GUI.changed)
				{
					EditorUtility.SetDirty(solver);
					EditorSceneManager.MarkSceneDirty(solver.gameObject.scene);
				}
				
				return;
			}

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