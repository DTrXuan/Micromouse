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
		public bool queued;
		public int value;
		public TextMesh textMesh;

		public FloodCell(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	private enum FloodStatus
	{
		Find,
		Return,
		Finished,
	}

	private Maze.Cell previousCell;
	private FloodCell[,] floodFill;
	private FloodStatus floodStatus;

	public enum ExploreAlgorithm
	{
		RightWall,
		FloodFill,
		ModifiedFloodFill,
	}
	
	public enum RunAlgorithm
	{
		TBD
	}

	[Flags]
	public enum MouseOrientation
	{
		North = 1 << 0, // 1
		East = 1 << 1, // 2
		South = 1 << 2, // 4
		West = 1 << 3, // 8
	}

	public static Maze.Wall GetWallFromMouseOrientation(MouseOrientation mouseOrientation)
	{
		switch (mouseOrientation)
		{
			case MouseOrientation.North:
				return Maze.Wall.North;

			case MouseOrientation.East:
				return Maze.Wall.East;

			case MouseOrientation.South:
				return Maze.Wall.South;

			case MouseOrientation.West:
				return Maze.Wall.West;
		}

		return Maze.Wall.None;
	}

	public float stepTime;
	public ExploreAlgorithm exploreAlgorithm;
	public RunAlgorithm runAlgorithm;
	public float sensorDetectionLength;

	private MouseOrientation orientation;
	private Maze.Cell[,] cells;
	
	private bool pauseExplore;
	private bool pauseRun;
	private int xPos, yPos;

	private bool forceMove;

	private void Start()
	{
		Reset();
	}

	private void Reset()
	{
		cells = new Maze.Cell[Maze.MazeSize.x, Maze.MazeSize.y];
		for(yPos = 0; yPos < Maze.MazeSize.y; yPos++)
		{
			for (xPos = 0; xPos < Maze.MazeSize.x; xPos++)
			{
				cells[xPos, yPos] = new Maze.Cell(xPos, yPos);
			}
		}

		Maze.Cell start = Maze.Cells[0, 0];
		orientation = MouseOrientation.North;
		if (start.HasWall(Maze.Wall.North))
			orientation = MouseOrientation.East;
		
		GameObject.FindGameObjectsWithTag("Wall").ToList().ForEach(go => go.GetComponent<Renderer>().material.color = Color.white);
		GameObject.FindGameObjectsWithTag("Ground").ToList().ForEach(go => go.GetComponent<Renderer>().material.color = Color.black);
		GameObject.FindGameObjectsWithTag("End").ToList().ForEach(go => go.GetComponent<Renderer>().material.color = Color.gray);

		xPos = 0;
		yPos = 0;

		UpdateTransform();
	}

	public void Explore()
	{
		Reset();

		pauseExplore = false;

		switch (exploreAlgorithm)
		{
			case ExploreAlgorithm.FloodFill:
				SetupFloodFill();
				break;
			case ExploreAlgorithm.ModifiedFloodFill:
				SetupModifiedFloodFill();
				break;
		}

		StartCoroutine(StepExplore());
	}

	private void SetupFloodFill()
	{
		floodFill = new FloodCell[Maze.MazeSize.x, Maze.MazeSize.y];
		floodStatus = FloodStatus.Find;
		
		for (int y = 0; y < Maze.MazeSize.y; y++)
		{
			for (int x = 0; x < Maze.MazeSize.x; x++)
			{
				floodFill[x, y] = new FloodCell(x, y);
				floodFill[x, y].textMesh = GameObject.Find(x + "," + y).GetComponentInChildren<TextMesh>();
			}
		}
		
		previousCell = null;
	}
	
	private void SetupModifiedFloodFill()
	{
		floodFill = new FloodCell[Maze.MazeSize.x, Maze.MazeSize.y];
		floodStatus = FloodStatus.Find;

		for (int y = 0; y < Maze.MazeSize.y; y++)
		{
			for (int x = 0; x < Maze.MazeSize.x; x++)
			{
				floodFill[x, y] = new FloodCell(x, y);
				floodFill[x, y].textMesh = GameObject.Find(x + "," + y).GetComponentInChildren<TextMesh>();
			}
		}

		CalculateFloodFill(Maze.GetEndCells());
		
		previousCell = null;
	}

	private void CalculateFloodFill(params Maze.Coord[] targetCells)
	{
		var currentStack = new Queue<Maze.Coord>();
		var nextStack = new Queue<Maze.Coord>();
		int currentFloodValue = 0;
		
		int columns = (int) Maze.MazeSize.x; 
		int rows = (int) Maze.MazeSize.y;

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < columns; x++)
			{
				floodFill[x, y].queued = false;
			}
		}

		for (int i = 0; i < targetCells.Length; i++)
			nextStack.Enqueue(targetCells[i]);

		while (nextStack.Count > 0)
		{
			currentStack = new Queue<Maze.Coord>(nextStack);
			nextStack.Clear();

			for(int i = 0; i < currentStack.Count; i++)
			{
				var coord = currentStack.Dequeue();
				floodFill[coord.x, coord.y].value = currentFloodValue;
				floodFill[coord.x, coord.y].textMesh.text = "" + currentFloodValue;
				floodFill[coord.x, coord.y].queued = true;
				currentStack.Enqueue(coord);
			}

			while (currentStack.Count > 0)
			{
				var coord = currentStack.Dequeue();

				// left
				if (coord.x > 0 && !floodFill[coord.x - 1, coord.y].queued && !cells[coord.x, coord.y].HasWall(Maze.Wall.West)) 
					nextStack.Enqueue(new Maze.Coord(coord.x - 1, coord.y));
				
				// right
				if (coord.x < columns - 1 && !floodFill[coord.x + 1, coord.y].queued && !cells[coord.x, coord.y].HasWall(Maze.Wall.East)) 
					nextStack.Enqueue(new Maze.Coord(coord.x + 1, coord.y));

				// down
				if (coord.y > 0 && !floodFill[coord.x, coord.y-1].queued && !cells[coord.x, coord.y].HasWall(Maze.Wall.South)) 
					nextStack.Enqueue(new Maze.Coord(coord.x, coord.y-1));

				// up
				if (coord.y < rows - 1 && !floodFill[coord.x, coord.y+1].queued && !cells[coord.x, coord.y].HasWall(Maze.Wall.North)) 
					nextStack.Enqueue(new Maze.Coord(coord.x, coord.y+1));
			}

			currentFloodValue++;
		}
	}

	private void CalculateModifiedFloodFill(params Maze.Coord[] targetCells)
	{
		var currentStack = new Stack<Maze.Coord>();
		
		int columns = (int) Maze.MazeSize.x; 
		int rows = (int) Maze.MazeSize.y;

		currentStack.Push(new Maze.Coord(xPos, yPos));

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < columns; x++)
			{
				floodFill[x, x].queued = false;
			}
		}

		while (currentStack.Count > 0)
		{
			Maze.Coord coord = currentStack.Pop();
			int neighborMinimumFloodValue = Int32.MaxValue;

			// left
			if (coord.x > 0 && !cells[coord.x, coord.y].HasWall(Maze.Wall.West))
				neighborMinimumFloodValue = Math.Min(neighborMinimumFloodValue, floodFill[coord.x - 1, coord.y].value);
				
			// right
			if (coord.x < columns - 1 && !cells[coord.x, coord.y].HasWall(Maze.Wall.East))
				neighborMinimumFloodValue = Math.Min(neighborMinimumFloodValue, floodFill[coord.x + 1, coord.y].value);

			// down
			if (coord.y > 0 && !cells[coord.x, coord.y].HasWall(Maze.Wall.South))
				neighborMinimumFloodValue = Math.Min(neighborMinimumFloodValue, floodFill[coord.x, coord.y - 1].value);

			// up
			if (coord.y < rows - 1 && !cells[coord.x, coord.y].HasWall(Maze.Wall.North))
				neighborMinimumFloodValue = Math.Min(neighborMinimumFloodValue, floodFill[coord.x, coord.y + 1].value);

			if (floodFill[xPos, yPos].value != neighborMinimumFloodValue + 1)
			{
				floodFill[xPos, yPos].value = neighborMinimumFloodValue + 1;
				floodFill[coord.x, coord.y].textMesh.text = "" + floodFill[coord.x, coord.y].value;
				
				// left
				if (coord.x > 0 && !floodFill[coord.x - 1, coord.y].queued && !cells[coord.x, coord.y].HasWall(Maze.Wall.West))
				{
					currentStack.Push(new Maze.Coord(coord.x - 1, coord.y));
					floodFill[coord.x - 1, coord.y].queued = true;
				}
				
				// right
				if (coord.x < columns - 1 && !floodFill[coord.x + 1, coord.y].queued && !cells[coord.x, coord.y].HasWall(Maze.Wall.East)) 
				{
					currentStack.Push(new Maze.Coord(coord.x + 1, coord.y));
					floodFill[coord.x + 1, coord.y].queued = true;
				}

				// down
				if (coord.y > 0 && !floodFill[coord.x, coord.y-1].queued && !cells[coord.x, coord.y].HasWall(Maze.Wall.South)) 
				{
					currentStack.Push(new Maze.Coord(coord.x, coord.y-1));
					floodFill[coord.x, coord.y-1].queued = true;
				}

				// up
				if (coord.y < rows - 1 && !floodFill[coord.x, coord.y+1].queued && !cells[coord.x, coord.y].HasWall(Maze.Wall.North)) 
				{
					currentStack.Push(new Maze.Coord(coord.x, coord.y+1));
					floodFill[coord.x, coord.y+1].queued = true;
				}
			}
		}
	}

	private IEnumerator StepExplore()
	{
		yield return new WaitWhile(() => pauseExplore);

		TouchCell();

		if (forceMove)
		{
			MoveForward();
			forceMove = false;
		}
		else
		{
			switch (exploreAlgorithm)
			{
				case ExploreAlgorithm.RightWall:
					RightWallStep();
					break;
					
				case ExploreAlgorithm.FloodFill:
					FloodFillStep();
					break;

				case ExploreAlgorithm.ModifiedFloodFill:
					ModifiedFloodFillStep();
					break;
			}
		}

		UpdateTransform();

		yield return new WaitForSeconds(stepTime);
		yield return StartCoroutine(StepExplore());
	}
	
	private void StopExplore()
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

		var hasFrontWall = cells[xPos, yPos].HasWall(front);
		var hasRightWall = cells[xPos, yPos].HasWall(right);

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

		if (groundTag == "End" && floodStatus == FloodStatus.Find)
			floodStatus = FloodStatus.Return;
		else if (groundTag == "Start" && floodStatus == FloodStatus.Return)
		{
			floodStatus = FloodStatus.Finished;
			pauseExplore = true;
			return;
		}

		if (floodStatus == FloodStatus.Find)
			CalculateFloodFill(Maze.GetEndCells());
		else if (floodStatus == FloodStatus.Return)
			CalculateFloodFill(new Maze.Coord(0, 0));

		var front = orientation;
		var left = GetOrientation(orientation, -1);
		var right = GetOrientation(orientation, 1);
		
		int deltaRotation = 1;

		var hasFrontWall = cells[xPos, yPos].HasWall(GetWallFromMouseOrientation(front));
		var hasLeftWall = cells[xPos, yPos].HasWall(GetWallFromMouseOrientation(left));
		var hasRightWall = cells[xPos, yPos].HasWall(GetWallFromMouseOrientation(right));
		
		int lowestFlood = Int32.MaxValue;

		Maze.Cell cell = null;

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

		previousCell = cells[xPos, yPos];

		Rotate(deltaRotation);
		forceMove = true;
	}

	
	private void ModifiedFloodFillStep()
	{
		var groundTag = DetectGround(paint: false);

		if (groundTag == "End" && floodStatus == FloodStatus.Find)
			floodStatus = FloodStatus.Return;
		else if (groundTag == "Start" && floodStatus == FloodStatus.Return)
		{
			floodStatus = FloodStatus.Finished;
			pauseExplore = true;
			return;
		}

		if (floodStatus == FloodStatus.Find)
			CalculateModifiedFloodFill(Maze.GetEndCells());
		else if (floodStatus == FloodStatus.Return)
			CalculateModifiedFloodFill(new Maze.Coord(0, 0));

		var front = orientation;
		var left = GetOrientation(orientation, -1);
		var right = GetOrientation(orientation, 1);
		
		int deltaRotation = 1;

		var hasFrontWall = cells[xPos, yPos].HasWall(GetWallFromMouseOrientation(front));
		var hasLeftWall = cells[xPos, yPos].HasWall(GetWallFromMouseOrientation(left));
		var hasRightWall = cells[xPos, yPos].HasWall(GetWallFromMouseOrientation(right));
		
		int lowestFlood = Int32.MaxValue;

		Maze.Cell cell = null;

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

		previousCell = cells[xPos, yPos];

		Rotate(deltaRotation);
		forceMove = true;
	}

	private Maze.Cell GetAdjacentCell(MouseOrientation orientation)
	{
		int xNew = xPos;
		int yNew = yPos;

		int length = 1;

		if ((orientation & MouseOrientation.North) != 0)
			yNew += 1;

		else if ((orientation & MouseOrientation.South) != 0)
			yNew -= length;

		if ((orientation & MouseOrientation.East) != 0)
			xNew += length;

		else if ((orientation & MouseOrientation.West) != 0)
			xNew -= length;

		if (xNew < 0 || xNew >= Maze.MazeSize.x)
			return null;
		
		if (yNew < 0 || yNew >= Maze.MazeSize.y)
			return null;

		return cells[xNew, yNew];
	}

	private void MoveForward(int length = 1)
	{
		if ((orientation & MouseOrientation.North) != 0)
		{
			yPos += length;
		}
		else if ((orientation & MouseOrientation.South) != 0)
		{
			yPos -= length;
		}

		if ((orientation & MouseOrientation.East) != 0)
		{
			xPos += length;
		}
		else if ((orientation & MouseOrientation.West) != 0)
		{
			xPos -= length;
		}
	}

	private void UpdateTransform()
	{
		var wallDelta = Maze.WallDimensions.x + Maze.PillarDimensions.x;

		Vector3 position = Vector3.zero;
		position += new Vector3(1, 0, 1) * Maze.WallDimensions.x / 2;
		position += new Vector3(1, 0, 1) * (Maze.PillarDimensions.x + Maze.PillarDimensions.z) / 2;
		position += Vector3.up * Maze.WallDimensions.y;
		position += Vector3.forward * yPos * wallDelta;
		position += Vector3.right * xPos * wallDelta;

		int index = Array.IndexOf(Enum.GetValues(typeof(MouseOrientation)), orientation);
		var rotation = Quaternion.Euler(0, 90 * (int) index, 0);

		this.transform.rotation = rotation;
		this.transform.position = position;
	}

	private void TouchCell()
	{
		cells[xPos, yPos].visited = true;

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
			if (hitInfo.transform.tag == "Start")
			{
				if(paint) hitInfo.transform.GetComponent<Renderer>().material.color = Color.Lerp(Color.black, Color.blue, 0.2f);
				groundTag = "Start";
			}
			else if (hitInfo.transform.tag == "Ground")
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

		if (Maze.Cells[xPos, yPos].HasWall(wall))
		{
			cells[xPos, yPos].InsertWall(wall);

			var adjacentCell = GetAdjacentCell(orientation);
			if(adjacentCell != null) 
				cells[adjacentCell.x, adjacentCell.y].InsertWall(Maze.GetOppositeWall(wall));

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
	
	private bool exploring;
	private bool running;

	[CustomEditor(typeof(MazeSolver))]
	private class MazeSolverEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var solver = (MazeSolver) target;

			GUI.enabled = true;

			solver.stepTime = EditorGUILayout.Slider(new GUIContent("Step Time (s)"), solver.stepTime, 0.01f, 1f);
			{
				EditorGUILayout.Space();
			}

			GUI.enabled = !solver.exploring && !solver.running;
			solver.sensorDetectionLength = EditorGUILayout.Slider(new GUIContent("Sensor Detection (m)"), solver.sensorDetectionLength, 0.01f, 0.30f);

			{
				EditorGUILayout.Space();
			}
			
			GUI.enabled = !Application.isPlaying || !solver.exploring;
			solver.exploreAlgorithm = (ExploreAlgorithm) EditorGUILayout.EnumPopup(new GUIContent("Explore Algorithm"), solver.exploreAlgorithm);
			
			GUI.enabled = Application.isPlaying && !solver.running;
			if (!solver.exploring)
			{
				if (GUILayout.Button("Explore"))
				{
					solver.pauseExplore = false;
					solver.Explore();
					solver.exploring = true;
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				{
					if (!solver.pauseExplore)
					{
						if (GUILayout.Button("Pause explore"))
							solver.pauseExplore = true;
					}
					else
					{
						if (GUILayout.Button("Resume explore"))
							solver.pauseExplore = false;
					}

					if (GUILayout.Button("Stop explore"))
					{
						solver.pauseExplore = true;
						solver.exploring = false;
						solver.StopExplore();
						solver.Reset();
					}
				}
				GUILayout.EndHorizontal();
			}

			GUI.enabled = Application.isPlaying && !solver.exploring;

			{
				EditorGUILayout.Space();
			}
			
			GUI.enabled = !Application.isPlaying || !solver.running;
			solver.runAlgorithm = (RunAlgorithm) EditorGUILayout.EnumPopup(new GUIContent("Run Algorithm"), solver.runAlgorithm);

			GUI.enabled = Application.isPlaying && !solver.exploring;

			if (!solver.running)
			{
				if (GUILayout.Button("Run"))
				{
					solver.pauseRun = false;
					solver.StepRun();
					solver.running = true;
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
						solver.running = false;
						solver.StopRun();
						solver.Reset();
					}
				}
				GUILayout.EndHorizontal();
			}

			if (!Application.isPlaying)
			{
				if(GUI.changed)
				{
					EditorUtility.SetDirty(solver);
					EditorSceneManager.MarkSceneDirty(solver.gameObject.scene);
				}
			}
		}
	}
}