using System;
using UnityEngine;

public static class Maze {

	public class Coord
	{
		public int x, y;

		public Coord(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public class Cell
	{
		public int x, y;
		public bool visited;
		public bool onStack;
		public Wall walls;

		private Cell() {}

		public Cell(int x, int y, Wall walls = Wall.None)
		{
			this.x = x;
			this.y = y;
			this.walls = walls;
			this.visited = false;
			this.onStack = false;
		}

		public bool HasWall(Wall wall)
		{
			return (walls & wall) != 0;
		}

		public bool RemoveWall(Wall wall)
		{
			if (HasWall(wall))
			{
				walls ^= wall;
				return true;
			}

			return false;
		}
		
		public bool InsertWall(Wall wall)
		{
			if (!HasWall(wall))
			{
				walls ^= wall;
				return true;
			}

			return false;
		}
	}

	[Flags]
	public enum Wall
	{
		None = 0,
		North = 1 << 0, // 1
		East  = 1 << 1, // 2
		South = 1 << 2, // 4
		West  = 1 << 3, // 8
		All   = North | South | East | West // 15
	}

	public static Wall GetOppositeWall(Wall wall)
	{
		switch(wall)
		{
			case Wall.None: return Wall.All;
			case Wall.All: return Wall.None;
			case Wall.North: return Wall.South;
			case Wall.South: return Wall.North;
			case Wall.East: return Wall.West;
			case Wall.West: return Wall.East;
		}

		return Wall.None;
	}

	public static Wall GetWallBetween(Maze.Coord currentCell, Maze.Coord nextCell)
	{
		Maze.Cell cellA = Maze.Cells[currentCell.x, currentCell.y];
		Maze.Cell cellB = Maze.Cells[nextCell.x, nextCell.y];
		
		if(cellA.x + 1 == cellB.x)
			if(cellA.HasWall(Maze.Wall.East) && cellB.HasWall(Maze.Wall.West))
				return Maze.Wall.East;

		if(cellB.x + 1 == cellA.x)
			if(cellA.HasWall(Maze.Wall.West) && cellB.HasWall(Maze.Wall.East))
				return Maze.Wall.West;

		if(cellA.y + 1 == cellB.y)
			if(cellA.HasWall(Maze.Wall.North) && cellB.HasWall(Maze.Wall.South))
				return Maze.Wall.North;

		if (cellB.y + 1 == cellA.y)
			if(cellA.HasWall(Maze.Wall.South) && cellB.HasWall(Maze.Wall.North))
				return Maze.Wall.South;

		return Maze.Wall.None;
	}

	public static void Clear()
	{
		Maze.Cells = new Maze.Cell[Maze.MazeSize.x, Maze.MazeSize.y];
		for (int y = 0; y < Maze.MazeSize.y; ++y)
		{
			for (int x = 0; x < Maze.MazeSize.x; ++x)
			{
				Maze.Cells[x, y] = new Maze.Cell(x, y, Maze.Wall.None);

				if (x == 0)
				{
					Maze.Cells[x, y].InsertWall(Maze.Wall.West);
				}

				if (y == 0)
				{
					Maze.Cells[x, y].InsertWall(Maze.Wall.South);
				}

				
				if (x == Maze.MazeSize.x - 1)
				{
					Maze.Cells[x, y].InsertWall(Maze.Wall.East);

				}
				if (y == Maze.MazeSize.y - 1)
				{
					Maze.Cells[x, y].InsertWall(Maze.Wall.North);
				}
			}
		}
	}

	public static Cell[,] Cells;
	public static Coord MazeSize;
	public static Coord EndSize;
	public static Coord EndDelta;
	public static Vector3 PillarDimensions, WallDimensions;

	public static Coord GetEndCoord()
	{		
		int x = (Maze.MazeSize.x - Maze.EndSize.x + 1) / 2 + Maze.EndDelta.x;
		int y = (Maze.MazeSize.y - Maze.EndSize.y + 1) / 2 + Maze.EndDelta.y;

		x = Math.Max(0, Math.Min(x, Maze.MazeSize.x - Maze.EndSize.x));
		y = Math.Max(0, Math.Min(y, Maze.MazeSize.y - Maze.EndSize.y));

		return new Coord(x, y);
	}

	public static Coord[] GetEndCells()
	{
		var coords = new Maze.Coord[Maze.EndSize.x * Maze.EndSize.y];

		var endCoord = Maze.GetEndCoord();

		int i = 0;

		for (int xAux = 0; xAux < Maze.EndSize.x; xAux++)
		{
			for (int yAux = 0; yAux < Maze.EndSize.y; yAux++)
			{
				coords[i] = new Maze.Coord(endCoord.x + xAux, endCoord.y + yAux);
				i++;
			}
		}

		return coords;
	}
}
