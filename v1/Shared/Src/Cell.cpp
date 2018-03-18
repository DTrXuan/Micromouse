#include "Cell.hpp"
#include "Config.hpp"
#include "Mouse.hpp"

Wall GetOppositeWall(Wall wall)
{
	if(wall == Wall::EAST)
		return Wall::WEST;

	if(wall == Wall::WEST)
		return Wall::EAST;

	if (wall == Wall::NORTH)
		return Wall::SOUTH;

	if(wall == Wall::SOUTH)
		return Wall::NORTH;

	return Wall::NONE;
}

Cell::Cell()
{
	Reset(false);
	_x = -1;
	_y = -1;
}

Cell::Cell(int x, int y)
{
	Reset(false);
	_x = x;
	_y = y;
}

void Cell::AddWall(Wall wall, bool detected)
{
	if(detected)
		_detectedWalls = _detectedWalls | wall;
	else
		_walls = _walls | wall;
}

void Cell::SetWall(Wall wall, bool detected)
{
	if(detected)
		_detectedWalls = wall;
	else
		_walls = wall;
}

void Cell::RemoveWall(Wall wall, bool detected)
{
	if(detected)
		_detectedWalls = _detectedWalls ^ ~wall;
	else
		_walls = _walls ^ ~wall;
}

bool Cell::HasWall(Wall wall, bool detected)
{
	if(detected)
		return (_detectedWalls & wall) != Wall::NONE;
	else
		return (_walls & wall) != Wall::NONE;
}

Wall Cell::GetWalls(bool detected)
{
	if(detected)
		return _detectedWalls;
	else
		return _walls;
}

bool Cell::IsVisited()
{
	return _visited;
}

void Cell::SetVisited(bool visited)
{
	_visited = visited;
}

bool Cell::IsSkipped()
{
	return _skipped;
}

void Cell::SetSkipped(bool skipped)
{
	_skipped = skipped;
}

bool Cell::IsUnreachable()
{
	return _unreachable;
}

void Cell::SetUnreachable(bool unreachable)
{
	_unreachable = unreachable;
}

int Cell::GetFloodfill()
{
	return _floodfill;
}

void Cell::SetFloodfill(int floodfill)
{
	_floodfill = floodfill;
}

int Cell::GetPositionX()
{
	return _x;
}

int Cell::GetPositionY()
{
	return _y;
}

void Cell::SetQueued(bool queued)
{
	_queued = queued;
}

bool Cell::GetQueued()
{
	return _queued;
}

void Cell::Reset(bool detected)
{
	_detectedWalls = Wall::NONE;
	_visited = false;
	_floodfill = 0;
	_queued = false;
	_skipped = false;
	_unreachable = false;

	if(detected == false)
		_walls = Wall::NONE;
}

bool Cell::IsStart()
{
	return _x == 0 && _y == 0;
}

bool Cell::IsTarget()
{
	for(Cell* cell : Mouse::Instance()->GetTargetCells())
	{
		if(_x == cell->GetPositionX() && _y == cell->GetPositionY())
			return true;
	}

	return false;
}

bool Cell::IsEnd()
{
	for(int i = 0; i < Config::EndCellsLength; i++)
	{
		if(_x == Config::EndCells[i * 2] && _y == Config::EndCells[i * 2 + 1])
			return true;
	}

	return false;
}
