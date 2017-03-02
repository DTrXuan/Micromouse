using UnityEditor;
using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
	public Vector2 MazeSize;
	public Vector3 WallDimensions;
	public Vector2 EndSize;

	private float wallDelta;
	private Vector3 originalCameraPosition;

	private GameObject horizontalWallPrimitive;
	private GameObject verticalWallPrimitive;
	private GameObject groundPrimitive;
	private GameObject endPrimitive;

	private MazeGenerator.Cell[,] maze;

	void Awake()
	{
		originalCameraPosition = Camera.main.transform.position;
	}

	public void Start()
	{
		Initialize();
		GenerateMaze();
		Dipose();
	}
	
	void Initialize()
	{
		Camera.main.transform.position = originalCameraPosition;

		wallDelta = WallDimensions.x - WallDimensions.z;

		horizontalWallPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		horizontalWallPrimitive.transform.localScale = WallDimensions;
		horizontalWallPrimitive.name = "Wall";
		horizontalWallPrimitive.tag = "Wall";

		verticalWallPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		verticalWallPrimitive.transform.localScale = new Vector3(WallDimensions.z, WallDimensions.y, WallDimensions.x);
		verticalWallPrimitive.name = "Wall";
		verticalWallPrimitive.tag = "Wall";
		
		groundPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		groundPrimitive.transform.localScale = new Vector3( WallDimensions.x, 0.1f, WallDimensions.x);
		groundPrimitive.GetComponent<Renderer>().material.color = Color.black;
		groundPrimitive.name = "Ground";
		groundPrimitive.tag = "Ground";
		
		endPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
		endPrimitive.transform.localScale = new Vector3(EndSize.x * wallDelta, 0.1f, EndSize.y * wallDelta);
		endPrimitive.GetComponent<Renderer>().material.color = Color.Lerp(Color.black, Color.gray, 0.33f);
		endPrimitive.name = "End";
		endPrimitive.tag = "End";
	}

	void GenerateMaze()
	{
		foreach(Transform child in transform)
		{
			Destroy(child.gameObject);
		}

		var generator = new MazeGenerator(MazeSize, EndSize);

		maze = generator.GenerateMaze();

		Vector3 horizontalOriginPosition = Vector3.zero;
		horizontalOriginPosition += new Vector3(0, 0, wallDelta / 2);
		horizontalOriginPosition += Vector3.up * WallDimensions.y;

		Vector3 verticalOriginPosition = Vector3.zero;
		verticalOriginPosition -= new Vector3(wallDelta / 2, 0, 0);
		verticalOriginPosition += Vector3.up * WallDimensions.y;

		for(int y = 0; y < MazeSize.y; y++)
		{
			for(int x = 0; x < MazeSize.x; x++)
			{
				if((maze[x,y].walls & MazeGenerator.Wall.North) != 0)
				{
					var newWall = Instantiate(horizontalWallPrimitive);
					newWall.transform.position = horizontalOriginPosition + Vector3.right * wallDelta * x;
					newWall.transform.position += Vector3.forward * wallDelta * (y+1);
					newWall.transform.parent = transform;
				}

				if ((maze[x,y].walls & MazeGenerator.Wall.West) != 0)
				{
					var newWall = Instantiate(verticalWallPrimitive);
					newWall.transform.position = verticalOriginPosition + Vector3.forward * wallDelta * (y+1);
					newWall.transform.position += Vector3.right * wallDelta * x;
					newWall.transform.parent = transform;
				}
				
				if(x == MazeSize.x - 1) // Last East
				{
					var newWall = Instantiate(verticalWallPrimitive);
					newWall.transform.position = verticalOriginPosition + Vector3.forward * wallDelta * (y+1);;
					newWall.transform.position += Vector3.right * wallDelta * (x+1);
					newWall.transform.parent = transform;
				}
				
				if(y == 0) // Last South
				{
					var newWall = Instantiate(horizontalWallPrimitive);
					newWall.transform.position = horizontalOriginPosition + Vector3.right * wallDelta * x;
					newWall.transform.position += Vector3.forward * wallDelta * y;
					newWall.transform.parent = transform;
				}

				
				var ground = Instantiate(groundPrimitive);
				ground.transform.position += Vector3.right * wallDelta * x;
				ground.transform.position += Vector3.forward * wallDelta * y;
				ground.transform.position += Vector3.forward * wallDelta;
				ground.transform.position += Vector3.down * WallDimensions.y / 2;
				ground.transform.parent = transform;
			}
		}
		
		var end = Instantiate(endPrimitive);
		end.transform.position += Vector3.right * wallDelta * MazeSize.x / 2;
		end.transform.position += Vector3.left * wallDelta / 2;
		end.transform.position += Vector3.forward * wallDelta * MazeSize.y / 2;
		end.transform.position += Vector3.forward * wallDelta / 2;
		end.transform.position += Vector3.down * WallDimensions.y / 2;
		end.transform.position += Vector3.up * 0.001f;
		end.transform.parent = transform;
		
		Camera.main.transform.position += Vector3.right * wallDelta * (MazeSize.x - 1) / 2;
		Camera.main.transform.position += Vector3.forward * wallDelta * (MazeSize.y + 1) / 2;
	}

	void Dipose()
	{
		Destroy(horizontalWallPrimitive);
		Destroy(verticalWallPrimitive);
		Destroy(groundPrimitive);
		Destroy(endPrimitive);
	}

	public MazeGenerator.Cell[,] GetMaze()
	{
		return maze;
	}
	
	[CustomEditor(typeof(MazeBuilder))]
	private class MazaeBuilderEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if(!Application.isPlaying)
				return;

			MazeBuilder builder = (MazeBuilder) target;
			
			if (GUILayout.Button("Generate"))
			{
				builder.Start();
			}
		}
	}
}
