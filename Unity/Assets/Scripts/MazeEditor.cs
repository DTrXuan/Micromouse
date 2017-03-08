using System;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MazeEditor : MonoBehaviour
{
	public bool isEditing;
    public float distanceToWallThreshold;

	public void Update()
	{
		if (isEditing && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100.0f))
			{
				ToggleWall(hit, insert: Input.GetMouseButton(0) && !Input.GetMouseButton(1));
			}
		}
	}

	public void ToggleWall(RaycastHit hit, bool insert)
	{
		var pillar1 = GetClosestPillar(hit.point);
		var pillar2 = GetClosestPillar(hit.point, pillar1);

		Vector3 bottomLeftPosition = new Vector3(Math.Min(pillar1.position.x, pillar2.position.x), 0, Math.Min(pillar1.position.z, pillar2.position.z));
		Vector3 topRightPosition = new Vector3(Math.Max(pillar1.position.x, pillar2.position.x), 0, Math.Max(pillar1.position.z, pillar2.position.z));

	    var distanceToWallCenter = Vector3.Distance(hit.point, (topRightPosition + bottomLeftPosition)/2);

        if (distanceToWallCenter > distanceToWallThreshold)
	        return;

        Maze.Coord bottomLeftCoord = new Maze.Coord(0, 0);
        bottomLeftCoord.x = (int)Math.Round((bottomLeftPosition.x) * Maze.MazeSize.x / (Camera.main.transform.position.x * 2));
        bottomLeftCoord.y = (int)Math.Round((bottomLeftPosition.z) * Maze.MazeSize.y / (Camera.main.transform.position.z * 2));

        Vector3 diffPillar = pillar2.position - pillar1.position;

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
			if (insert)
			{
				Maze.Cells[c0.x, c0.y].InsertWall(wall);
				Maze.Cells[c1.x, c1.y].InsertWall(Maze.GetOppositeWall(wall));
			}
			else
			{
				Maze.Cells[c0.x, c0.y].RemoveWall(wall);
				Maze.Cells[c1.x, c1.y].RemoveWall(Maze.GetOppositeWall(wall));
			}

			MazeSerializer.ResetMazeName();
			FindObjectOfType<MazeBuilder>().Render();
		}
	}

	private GameObject[] pillars;

	private Transform GetClosestPillar(Vector3 clickPoint, Transform excludePillar = null)
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
		    var editor = (MazeEditor) target;

            editor.distanceToWallThreshold = EditorGUILayout.Slider(new GUIContent("Distance Threshold (m)"), editor.distanceToWallThreshold, 0.01f, 0.20f);

			GUI.enabled = Application.isPlaying;

			if (!editor.isEditing)
			{
				if (GUILayout.Button("Start editing"))
				{
					MazeSerializer.ResetMazeName();
                    editor.isEditing = true;
				}
			}
			else
			{
				if (GUILayout.Button("Stop editing"))
				{
                    editor.isEditing = false;
				}
			}

            if (!Application.isPlaying)
            {
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(editor);
                    EditorSceneManager.MarkSceneDirty(editor.gameObject.scene);
                }
            }
        }
    }
}
