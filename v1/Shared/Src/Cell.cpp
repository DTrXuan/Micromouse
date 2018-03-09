#include "Cell.hpp"

Cell::Cell()
{
	_x = -1;
	_y = -1;
	_walls = Wall::NONE;
}

Cell::Cell(int x, int y)
{
	_x = x;
	_y = y;
	_walls = Wall::NONE;
}

void Cell::AddWall(Wall wall)
{
	_walls = _walls | wall;
}

void Cell::RemoveWall(Wall wall)
{
	_walls = _walls ^ ~wall;
}
