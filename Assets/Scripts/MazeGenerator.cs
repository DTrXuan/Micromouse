// Find the center of a maze, then roam back and forth along the shortest route forever.
// Dan Royer (dan@marginallyclever.com) 2016-05-21
// Maze creation and access methods
using UnityEngine;

public class MazeGenerator
{
	class Coord
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
	
	[System.Flags]
	public enum Wall
	{
		None = 0,
		North = 1 << 0, // 1
		South = 1 << 1, // 2
		East  = 1 << 2, // 4
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

	Cell[,] cells;
	int columns, rows;
	int endColumns, endRows;

	public MazeGenerator(Vector2 mazeSize, Vector2 endSize)
	{
		this.columns = (int)mazeSize.x;
		this.rows = (int)mazeSize.y;
		
		this.endColumns = (int)endSize.x;
		this.endRows = (int)endSize.y;
	}

	/**
	 * build a list of walls in the maze, cells in the maze, and how they connect to each other.
	 * @param out
	 * @throws IOException
	 */
	public Cell[,] GenerateMaze()
	{
		// build the cells
		cells = new Cell[columns, rows];

		for (int y = 0; y < rows; ++y)
		{
			for (int x = 0; x < columns; ++x)
			{
				cells[x, y] = new Cell(x, y, Wall.All);
			}
		}

		int unvisitedCells = cells.Length; // -1 for initial cell.
		int cellsOnStack = 0;

		// Make the initial cell the current cell and mark it as visited
		Coord currentCell = new Coord(0, 0);
		cells[0,0].visited = true;
		--unvisitedCells;

		// While there are unvisited cells
		while (unvisitedCells > 0)
		{
			// If the current cell has any neighbours which have not been visited
			// Choose randomly one of the unvisited neighbours
			Coord nextCell = chooseUnvisitedNeighbor(currentCell);
			if (nextCell != null)
			{
				int cx = currentCell.x;
				int cy = currentCell.y;

				int nx = nextCell.x;
				int ny = nextCell.y;

				// Push the current cell to the stack
				cells[cx, cy].onStack = true;
				++cellsOnStack;

				// Remove the wall between the current cell and the chosen cell
				Wall wall = findWallBetween(currentCell, nextCell);
				if(wall != Wall.None)
				{
					cells[cx, cy].RemoveWall(wall);
					cells[nx, ny].RemoveWall(GetOppositeWall(wall));
				}

				// Make the chosen cell the current cell and mark it as visited
				currentCell = nextCell;
				cx = currentCell.x;
				cy = currentCell.y;

				cells[cx, cy].visited = true;
				--unvisitedCells;
			}
			else if (cellsOnStack > 0)
			{
				// else if stack is not empty pop a cell from the stack
				for (int y = 0; y < rows; ++y)
				{
					for (int x = 0; x < columns; ++x)
					{
						if (cells[x, y].onStack)
						{
							// Make it the current cell
							currentCell = new Coord(x, y);
							cells[x, y].onStack = false;
							--cellsOnStack;
							goto breakLoops;
						}
					}
				}
				
				breakLoops: ;
			}
		}

		// remove the walls between the end squares
		Coord auxCurrentCell, auxNextCell;
		Wall auxWall;
		
		int x0 = columns / 2 - endColumns / 2;
		int y0 = rows / 2 - endRows / 2;

		for(int xAux = 0; xAux < endColumns; xAux++)
		{
			for(int yAux = 0; yAux < endRows - 1; yAux++)
			{
				currentCell = new Coord(x0 + xAux, y0 + yAux);
				auxCurrentCell = currentCell; auxNextCell = new Coord(currentCell.x, currentCell.y + 1);

				auxWall = findWallBetween(auxCurrentCell, auxNextCell);
				if(auxWall != Wall.None)
				{
					cells[auxCurrentCell.x, auxCurrentCell.y].walls ^= auxWall;
					cells[auxNextCell.x, auxNextCell.y].walls  ^= GetOppositeWall(auxWall);
				}
			}
		}

		for(int xAux = 0; xAux < endColumns - 1; xAux++)
		{
			for(int yAux = 0; yAux < endRows; yAux++)
			{
				currentCell = new Coord(x0 + xAux, y0 + yAux);
				auxCurrentCell = currentCell; auxNextCell = new Coord(currentCell.x + 1, currentCell.y);

				auxWall = findWallBetween(auxCurrentCell, auxNextCell);
				if(auxWall != Wall.None)
				{
					cells[auxCurrentCell.x, auxCurrentCell.y].walls ^= auxWall;
					cells[auxNextCell.x, auxNextCell.y].walls  ^= GetOppositeWall(auxWall);
				}
			}
		}

		return cells;
	}

	Coord chooseUnvisitedNeighbor(Coord currentCell)
	{
		int x = currentCell.x;
		int y = currentCell.y;

		Coord[] candidates = new Coord[4];
		int found = 0;

		// left
		if (x > 0 && cells[x-1, y].visited == false)
		{
			candidates[found++] = new Coord(x-1, y);
		}
		// right
		if (x < columns - 1 && cells[x+1, y].visited == false)
		{
			candidates[found++] = new Coord(x+1, y);
		}
		// down
		if (y > 0 && cells[x, y-1].visited == false)
		{
			candidates[found++] = new Coord(x, y-1);
		}
		// up
		if (y < rows - 1 && cells[x, y+1].visited == false)
		{
			candidates[found++] = new Coord(x, y+1);
		}

		if (found == 0)
			return null;

		// choose a random candidate
		int choice = (int) (UnityEngine.Random.Range(0f, 1f) * found);

		return candidates[choice];
	}

	/**
	 * Find the index of the wall between two cells
	 * returns -1 if no wall is found (asking the impossible)
	 */
	Wall findWallBetween(Coord currentCell, Coord nextCell)
	{
		Cell cellA = cells[currentCell.x, currentCell.y];
		Cell cellB = cells[nextCell.x, nextCell.y];
		
		if(cellA.x + 1 == cellB.x)
			if(cellA.HasWall(Wall.East) && cellB.HasWall(Wall.West))
				return Wall.East;

		if(cellB.x + 1 == cellA.x)
			if(cellA.HasWall(Wall.West) && cellB.HasWall(Wall.East))
				return Wall.West;

		if(cellA.y + 1 == cellB.y)
			if(cellA.HasWall(Wall.North) && cellB.HasWall(Wall.South))
				return Wall.North;

		if (cellB.y + 1 == cellA.y)
			if(cellA.HasWall(Wall.South) && cellB.HasWall(Wall.North))
				return Wall.South;

		return Wall.None;
	}
}
