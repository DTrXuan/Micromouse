using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class MazeEditor : MonoBehaviour
{
	private static bool isEditing;

	public void Update()
	{
		if (isEditing && Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100.0f))
			{
				ToggleWall(hit);
			}
		}
	}

	public void ToggleWall(RaycastHit hit)
	{
		var pillar1 = GetClosestPillar(hit.point);
		var pillar2 = GetClosestPillar(hit.point, pillar1);

		Vector3 diffPillar = pillar2.position - pillar1.position;

		Vector2 bottomLeftPosition = new Vector2(Math.Min(pillar1.position.x, pillar2.position.x), Math.Min(pillar1.position.z, pillar2.position.z));
		Maze.Coord bottomLeftCoord = new Maze.Coord(0, 0);
		bottomLeftCoord.x = (int) Math.Round((bottomLeftPosition.x) * Maze.MazeSize.x / (Camera.main.transform.position.x * 2));
		bottomLeftCoord.y = (int) Math.Round((bottomLeftPosition.y) * Maze.MazeSize.y / (Camera.main.transform.position.z * 2));
		
		Vector2 topRightPosition = new Vector2(Math.Max(pillar1.position.x, pillar2.position.x), Math.Max(pillar1.position.z, pillar2.position.z));
		Maze.Coord topRightCoord = new Maze.Coord(0, 0);
		topRightCoord.x = (int) Math.Round((topRightPosition.x) * Maze.MazeSize.x / (Camera.main.transform.position.x * 2));
		topRightCoord.y = (int) Math.Round((topRightPosition.y) * Maze.MazeSize.y / (Camera.main.transform.position.z * 2));

		Maze.Coord c0 = null;
		Maze.Coord c1 = null;
		Maze.Wall wall = Maze.Wall.None;

		// vertical wall
		if (diffPillar.x == 0
			&& bottomLeftCoord.x > 0 && bottomLeftCoord.y >= 0
			&& bottomLeftCoord.x < Maze.MazeSize.x && bottomLeftCoord.y < Maze.MazeSize.y)
		{
			c0 = new Maze.Coord(bottomLeftCoord.x - 1, bottomLeftCoord.y);
			c1 = new Maze.Coord(bottomLeftCoord.x, bottomLeftCoord.y);

			wall = Maze.Wall.East;
		}
		// horizontal wall
		else if (diffPillar.z == 0 
			&& bottomLeftCoord.x >= 0 && bottomLeftCoord.y > 0
			&& bottomLeftCoord.x < Maze.MazeSize.x && bottomLeftCoord.y < Maze.MazeSize.y)
		{
			c0 = new Maze.Coord(bottomLeftCoord.x, bottomLeftCoord.y - 1);
			c1 = new Maze.Coord(bottomLeftCoord.x, bottomLeftCoord.y);

			wall = Maze.Wall.North;
		}

		if (c0 != null && c1 != null)
		{
			if (Maze.Cells[c0.x, c0.y].HasWall(wall))
			{
				Maze.Cells[c0.x, c0.y].RemoveWall(wall);
				Maze.Cells[c1.x, c1.y].RemoveWall(Maze.GetOppositeWall(wall));
			}
			else
			{
				Maze.Cells[c0.x, c0.y].InsertWall(wall);
				Maze.Cells[c1.x, c1.y].InsertWall(Maze.GetOppositeWall(wall));
			}

			MazeSerializer.ResetMazeName();
			FindObjectOfType<MazeBuilder>().Render();
		}
	}

	private GameObject[] pillars;

	Transform GetClosestPillar(Vector3 clickPoint, Transform excludePillar = null)
	{
		Transform tMin = null;
		float minDist = Mathf.Infinity;

		var pillars = GameObject.FindGameObjectsWithTag("Pillar");

		foreach (GameObject p in pillars)
		{
			float dist = Vector3.Distance(p.transform.position, clickPoint);
			if (dist < minDist && p.transform != excludePillar)
			{
				tMin = p.transform;
				minDist = dist;
			}
		}

		return tMin;
	}

	[CustomEditor(typeof(MazeEditor))]
	private class MazeEditorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			GUI.enabled = Application.isPlaying;
			
			EditorGUILayout.LabelField("");

			if (!MazeEditor.isEditing)
			{
				if (GUILayout.Button("Start editing"))
				{
					MazeSerializer.ResetMazeName();
					MazeEditor.isEditing = true;
				}
			}
			else
			{
				if (GUILayout.Button("Stop editing"))
				{
					MazeEditor.isEditing = false;
				}
			}
		}
	}
}