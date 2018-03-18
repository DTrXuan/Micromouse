#ifndef MAZE_H
#define MAZE_H

#include "Cell.hpp"
#include "Config.hpp"
#include "vector"

class Maze
{
private:
	/* Here will be the instance stored. */
	static Maze* instance;

	/* Private constructor to prevent instancing. */
	Maze();

	Cell** _cells;

public:
    /* Static access method. */
	static Maze* Instance();

	void Reset(bool detected);

	void ResetFloodfill();
	void StepFloodfill();
	void AddCellWall(Cell* cell, Wall wall, bool detected);

	Cell* GetCell(int x, int y);

	int GetDistanceToEnd(int x, int y);

	void CalculateFloodFill(std::vector<Cell*> targetCells);

	std::vector<Cell*> GetNeighbors(Cell* cell, bool detected);
};

#endif
