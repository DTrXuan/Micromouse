using System;
using UnityEditor;
using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
	private float wallDelta;
	private Vector3 originalCameraPosition;

	private GameObject horizontalWallPrimitive;
	private GameObject verticalWallPrimitive;
	private GameObject pillarPrimitive;
	private GameObject groundPrimitive;
	private GameObject endPrimitive;

	[SerializeField]
	private Vector2 _mazeSize;
	[SerializeField]
	private Vector3 _wallDimensions;
	[SerializeField]
	private Vector3 _pillarDimensions;
	[SerializeField]
	private Vector2 _endSize;
	[SerializeField]
	private Vector2 _endDelta;

	void Awake()
	{
		originalCameraPosition = Camera.main.transform.position;
	}

	void Start()
	{
		SetupValues();
		GenerateMaze();
		Dipose();
	}

	void Clear()
	{
		SetupValues();
		Maze.Clear();
		Render();
	}
	
	void Initialize()
	{
		Camera.main.transform.position = originalCameraPosition;
		
		wallDelta = Maze.WallDimensions.x + Maze.PillarDimensions.x;

		horizontalWallPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		horizontalWallPrimitive.transform.localScale = new Vector3(Maze.WallDimensions.x, Maze.WallDimensions.y, Maze.WallDimensions.z);
		horizontalWallPrimitive.name = "Wall";
		horizontalWallPrimitive.tag = "Wall";

		verticalWallPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		verticalWallPrimitive.transform.localScale = new Vector3(Maze.WallDimensions.z, Maze.WallDimensions.y, Maze.WallDimensions.x);
		verticalWallPrimitive.name = "Wall";
		verticalWallPrimitive.tag = "Wall";

		pillarPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		pillarPrimitive.transform.localScale = new Vector3(Maze.WallDimensions.z, Maze.WallDimensions.y, Maze.WallDimensions.z);
		pillarPrimitive.name = "Pillar";
		pillarPrimitive.tag = "Pillar";

		groundPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		groundPrimitive.transform.localScale = new Vector3(Maze.WallDimensions.x + Maze.PillarDimensions.x, 0.1f, Maze.WallDimensions.x + Maze.PillarDimensions.z);
		groundPrimitive.GetComponent<Renderer>().material.color = Color.black;
		groundPrimitive.name = "Ground";
		groundPrimitive.tag = "Ground";
		
		endPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		endPrimitive.transform.localScale = new Vector3(Maze.EndSize.x * wallDelta, 0.1f, Maze.EndSize.y * wallDelta);
		endPrimitive.GetComponent<Renderer>().material.color = Color.gray;
		endPrimitive.name = "End";
		endPrimitive.tag = "End";
	}

	void SetupValues()
	{
		Maze.MazeSize = new Maze.Coord((int) _mazeSize.x, (int) _mazeSize.y);
		Maze.EndSize = new Maze.Coord((int) _endSize.x, (int) _endSize.y);
		Maze.EndDelta = new Maze.Coord((int) _endDelta.x, (int) _endDelta.y);
		Maze.WallDimensions = _wallDimensions;
		Maze.PillarDimensions = _pillarDimensions;
	}

	void GenerateMaze()
	{
		var generator = new MazeGenerator();
		generator.GenerateMaze();

		MazeSerializer.ResetMazeName();
	
		Render();
	}

	public void Render()
	{
		Initialize();
		DrawMaze();
		Dipose();
	}

	void DrawMaze()
	{
		foreach(Transform child in transform)
			Destroy(child.gameObject);

		Vector3 horizontalOriginPosition = Vector3.zero;
		horizontalOriginPosition += new Vector3(Maze.WallDimensions.x/ 2 + Maze.PillarDimensions.x, 0, Maze.WallDimensions.z / 2);
		horizontalOriginPosition += Vector3.up * Maze.WallDimensions.y;
		
		Vector3 verticalOriginPosition = Vector3.zero;
		verticalOriginPosition += new Vector3(Maze.WallDimensions.z / 2, 0, Maze.WallDimensions.x / 2 + Maze.PillarDimensions.x);
		verticalOriginPosition += Vector3.up * Maze.WallDimensions.y;

		Vector3 pillarOriginPosition = Vector3.zero;
		pillarOriginPosition += new Vector3(Maze.PillarDimensions.z / 2, 0, Maze.PillarDimensions.x / 2);
		pillarOriginPosition += Vector3.up * Maze.PillarDimensions.y;

		for(int y = 0; y < Maze.MazeSize.y; y++)
		{
			for(int x = 0; x < Maze.MazeSize.x; x++)
			{
				var newPillar = Instantiate(pillarPrimitive);
				newPillar.transform.position = pillarOriginPosition + Vector3.forward * wallDelta * y;
				newPillar.transform.position += Vector3.right * wallDelta * x;
				newPillar.transform.parent = transform;

				if ((Maze.Cells[x,y].walls & Maze.Wall.West) != 0)
				{
					var newWall = Instantiate(verticalWallPrimitive);
					newWall.transform.position = verticalOriginPosition + Vector3.forward * wallDelta * y;
					newWall.transform.position += Vector3.right * wallDelta * x;
					newWall.transform.parent = transform;
				}
				
				if((Maze.Cells[x,y].walls & Maze.Wall.South) != 0)
				{
					var newWall = Instantiate(horizontalWallPrimitive);
					newWall.transform.position = horizontalOriginPosition + Vector3.right * wallDelta * x;
					newWall.transform.position += Vector3.forward * wallDelta * y;
					newWall.transform.parent = transform;
				}

				if(x == Maze.MazeSize.x - 1) // Last East
				{
					var newWall = Instantiate(verticalWallPrimitive);
					newWall.transform.position = verticalOriginPosition + Vector3.forward * wallDelta * y;
					newWall.transform.position += Vector3.right * wallDelta * (x+1);
					newWall.transform.parent = transform;

					newPillar = Instantiate(pillarPrimitive);
					newPillar.transform.position = pillarOriginPosition + Vector3.forward * wallDelta * y;
					newPillar.transform.position += Vector3.right * wallDelta * (x+1);
					newPillar.transform.parent = transform;
				}
				
				if(y == Maze.MazeSize.y - 1) // Last North
				{
					var newWall = Instantiate(horizontalWallPrimitive);
					newWall.transform.position = horizontalOriginPosition + Vector3.right * wallDelta * x;
					newWall.transform.position += Vector3.forward * wallDelta * (y+1);
					newWall.transform.parent = transform;
					
					newPillar = Instantiate(pillarPrimitive);
					newPillar.transform.position = pillarOriginPosition + Vector3.forward * wallDelta * (y+1);
					newPillar.transform.position += Vector3.right * wallDelta * x;
					newPillar.transform.parent = transform;

					if(x == Maze.MazeSize.x - 1) // Last East
					{
						newPillar = Instantiate(pillarPrimitive);
						newPillar.transform.position = pillarOriginPosition + Vector3.forward * wallDelta * (y+1);
						newPillar.transform.position += Vector3.right * wallDelta * (x+1);
						newPillar.transform.parent = transform;
					}
				}
				
				var ground = Instantiate(groundPrimitive);

				ground.transform.position += Vector3.right * wallDelta * x;
				ground.transform.position += Vector3.right * wallDelta / 2;
				ground.transform.position += Vector3.forward * wallDelta * y;
				ground.transform.position += Vector3.forward * wallDelta / 2;
				ground.transform.position += Vector3.down * Maze.WallDimensions.y / 2;
				ground.transform.parent = transform;

				if (x == 0 && y == 0)
				{
					ground.tag = "Start";
				}
			}
		}
		
		int x0, y0;
		Maze.GetEndPosition(out x0, out y0);

		var end = Instantiate(endPrimitive);
		end.transform.position += Vector3.right * wallDelta * x0;
		end.transform.position += Vector3.right * end.transform.localScale.x / 2;
		end.transform.position += Vector3.forward * wallDelta * y0;
		end.transform.position += Vector3.forward * end.transform.localScale.z / 2;
		end.transform.position += Vector3.down * Maze.WallDimensions.y / 2;
		end.transform.position += Vector3.up * 0.001f;
		
		end.transform.parent = transform;
		
		Camera.main.transform.position += Vector3.right * wallDelta * Maze.MazeSize.x / 2;
		Camera.main.transform.position += Vector3.forward * wallDelta * Maze.MazeSize.y / 2;
	}

	void Dipose()
	{
		Destroy(horizontalWallPrimitive);
		Destroy(verticalWallPrimitive);
		Destroy(pillarPrimitive);
		Destroy(groundPrimitive);
		Destroy(endPrimitive);
	}
	
	[CustomEditor(typeof(MazeBuilder))]
	private class MazaeBuilderEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUI.enabled = Application.isPlaying;

			MazeBuilder builder = (MazeBuilder) target;
			
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Clear"))
				{
					builder.Clear();
				}

				if (GUILayout.Button("Generate"))
				{
					builder.Start();
				}
			}
			GUILayout.EndHorizontal();
		}
	}
}
