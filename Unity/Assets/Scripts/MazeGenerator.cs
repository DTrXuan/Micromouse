// Find the center of a maze, then roam back and forth along the shortest route forever.
// Dan Royer (dan@marginallyclever.com) 2016-05-21
// Maze creation and access methods

using System;
using UnityEngine;

public class MazeGenerator
{	
	/**
	 * build a list of walls in the maze, cells in the maze, and how they connect to each other.
	 * @param out
	 * @throws IOException
	 */
	public void GenerateMaze()
	{
		// build the cells
		Maze.Cells = new Maze.Cell[Maze.MazeSize.x, Maze.MazeSize.y];

		for (int y = 0; y < Maze.MazeSize.y; ++y)
		{
			for (int x = 0; x < Maze.MazeSize.x; ++x)
			{
				Maze.Cells[x, y] = new Maze.Cell(x, y, Maze.Wall.All);
			}
		}

		int unvisitedCells = Maze.Cells.Length; // -1 for initial cell.
		int cellsOnStack = 0;

		// Make the initial cell the current cell and mark it as visited
		Maze.Coord currentCell = new Maze.Coord(0, 0);
		Maze.Cells[0,0].visited = true;
		--unvisitedCells;

		// While there are unvisited cells
		while (unvisitedCells > 0)
		{
			// If the current cell has any neighbours which have not been visited
			// Choose randomly one of the unvisited neighbours
			Maze.Coord nextCell = chooseUnvisitedNeighbor(currentCell);
			if (nextCell != null)
			{
				int cx = currentCell.x;
				int cy = currentCell.y;

				int nx = nextCell.x;
				int ny = nextCell.y;

				// Push the current cell to the stack
				Maze.Cells[cx, cy].onStack = true;
				++cellsOnStack;

				// Remove the wall between the current cell and the chosen cell
				Maze.Wall wall = Maze.GetWallBetween(currentCell, nextCell);
				if(wall != Maze.Wall.None)
				{
					Maze.Cells[cx, cy].RemoveWall(wall);
					Maze.Cells[nx, ny].RemoveWall(Maze.GetOppositeWall(wall));
				}

				// Make the chosen cell the current cell and mark it as visited
				currentCell = nextCell;
				cx = currentCell.x;
				cy = currentCell.y;

				Maze.Cells[cx, cy].visited = true;
				--unvisitedCells;
			}
			else if (cellsOnStack > 0)
			{
				// else if stack is not empty pop a cell from the stack
				for (int y = 0; y < Maze.MazeSize.y; ++y)
				{
					for (int x = 0; x < Maze.MazeSize.x; ++x)
					{
						if (Maze.Cells[x, y].onStack)
						{
							// Make it the current cell
							currentCell = new Maze.Coord(x, y);
							Maze.Cells[x, y].onStack = false;
							--cellsOnStack;
							goto breakLoops;
						}
					}
				}
				
				breakLoops: ;
			}
		}

		// remove the walls between the end squares
		Maze.Coord auxCurrentCell, auxNextCell;
		Maze.Wall auxWall;

		var endCoord = Maze.GetEndCoord();

		for(int xAux = 0; xAux < Maze.EndSize.x; xAux++)
		{
			for(int yAux = 0; yAux < Maze.EndSize.y - 1; yAux++)
			{
				currentCell = new Maze.Coord(endCoord.x + xAux, endCoord.y + yAux);
				auxCurrentCell = currentCell; auxNextCell = new Maze.Coord(currentCell.x, currentCell.y + 1);

				auxWall = Maze.GetWallBetween(auxCurrentCell, auxNextCell);
				if(auxWall != Maze.Wall.None)
				{
					Maze.Cells[auxCurrentCell.x, auxCurrentCell.y].walls ^= auxWall;
					Maze.Cells[auxNextCell.x, auxNextCell.y].walls  ^= Maze.GetOppositeWall(auxWall);
				}
			}
		}

		for(int xAux = 0; xAux < Maze.EndSize.x - 1; xAux++)
		{
			for(int yAux = 0; yAux < Maze.EndSize.y; yAux++)
			{
				currentCell = new Maze.Coord(endCoord.x + xAux, endCoord.y + yAux);
				auxCurrentCell = currentCell; auxNextCell = new Maze.Coord(currentCell.x + 1, currentCell.y);

				auxWall = Maze.GetWallBetween(auxCurrentCell, auxNextCell);
				if(auxWall != Maze.Wall.None)
				{
					Maze.Cells[auxCurrentCell.x, auxCurrentCell.y].walls ^= auxWall;
					Maze.Cells[auxNextCell.x, auxNextCell.y].walls  ^= Maze.GetOppositeWall(auxWall);
				}
			}
		}
	}

	Maze.Coord chooseUnvisitedNeighbor(Maze.Coord currentCell)
	{
		int x = currentCell.x;
		int y = currentCell.y;

		Maze.Coord[] candidates = new Maze.Coord[4];
		int found = 0;

		// left
		if (x > 0 && Maze.Cells[x-1, y].visited == false)
		{
			candidates[found++] = new Maze.Coord(x-1, y);
		}
		// right
		if (x < Maze.MazeSize.x - 1 && Maze.Cells[x+1, y].visited == false)
		{
			candidates[found++] = new Maze.Coord(x+1, y);
		}
		// down
		if (y > 0 && Maze.Cells[x, y-1].visited == false)
		{
			candidates[found++] = new Maze.Coord(x, y-1);
		}
		// up
		if (y < Maze.MazeSize.y - 1 && Maze.Cells[x, y+1].visited == false)
		{
			candidates[found++] = new Maze.Coord(x, y+1);
		}

		if (found == 0)
			return null;

		// choose a random candidate
		int choice = (int) (UnityEngine.Random.Range(0f, 1f) * found);

		return candidates[choice];
	}
}
