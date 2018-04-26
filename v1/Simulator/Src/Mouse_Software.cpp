#ifdef MOUSE_SOFTWARE

#include "iostream"
#include "cmath"
#include "vector"
#include <algorithm>

#include "Mouse.hpp"
#include "Utils.hpp"
#include "Config.hpp"
#include "Maze.hpp"

Mouse* Mouse::instance = 0;

Mouse* Mouse::Instance() {
	if (instance == 0) {
		instance = new Mouse();
	}

	return instance;
}

Mouse::Mouse() {
	Reset();
}

void Mouse::Loop() {

}

void Mouse::Setup() {

}

int Mouse::GetBattery() {
	return 100;
}

float Mouse::GetPositionX() {
	return _x;
}

float Mouse::GetPositionY() {
	return _y;
}

void Mouse::SetPosition(float x, float y) {
	_x = x;
	_y = y;
}

float Mouse::GetRotation() {
	return _rotation;
}

void Mouse::SetRotation(float rotation) {
	_rotation = rotation;
}

void Mouse::TriggerSensors() {
	Maze* maze = Maze::Instance();

	Cell* cell = maze->GetCell(_x, _y);
	cell->SetVisited(true);

	if (cell->HasWall(Wall::NORTH, false))
		maze->AddCellWall(cell, Wall::NORTH, true);
	if (cell->HasWall(Wall::SOUTH, false))
		maze->AddCellWall(cell, Wall::SOUTH, true);
	if (cell->HasWall(Wall::EAST, false))
		maze->AddCellWall(cell, Wall::EAST, true);
	if (cell->HasWall(Wall::WEST, false))
		maze->AddCellWall(cell, Wall::WEST, true);
}

bool Mouse::SortFloodfillCells(Cell* a, Cell* b)
{
	bool aVisitable = (a->IsVisited() || a->IsSkipped()) == false;
	bool bVisitable = (b->IsVisited() || b->IsSkipped()) == false;

	int aFlood = a->GetFloodfill();
	int bFlood = b->GetFloodfill();

	if(aFlood != bFlood)
		return aFlood < bFlood;

	if(aVisitable != bVisitable)
		return aVisitable;

	if(Mouse::Instance()->IsRotatedTowards(a))
		return true;
	else if(Mouse::Instance()->IsRotatedTowards(b))
		return false;

	return true;
}

void Mouse::StepMotion() {
	float dx = cos((_rotation - 90 ) * M_PI / 180);
	float dy = -sin((_rotation - 90) * M_PI / 180);

	if(_state == State::SEARCHING)
	{
		Cell *northCell, *southCell, *eastCell, *westCell, *currentCell;
		currentCell = Maze::Instance()->GetCell(_x, _y);

		currentCell->SetVisited(true);
		std::vector<Cell*> nextCells;

		int currentFloodfill;

		if(_y < Config::Height - 1)
		{
			northCell = Maze::Instance()->GetCell(_x, _y + 1);

			if(currentCell->HasWall(Wall::NORTH, true) == false)
				nextCells.push_back(northCell);
		}

		if(_y > 0)
		{
			southCell = Maze::Instance()->GetCell(_x, _y - 1);

			if(currentCell->HasWall(Wall::SOUTH, true) == false)
				nextCells.push_back(southCell);
		}

		if(_x < Config::Width - 1)
		{
			eastCell = Maze::Instance()->GetCell(_x + 1, _y);

			if(currentCell->HasWall(Wall::EAST, true) == false)
				nextCells.push_back(eastCell);
		}

		if(_x > 0)
		{
			westCell = Maze::Instance()->GetCell(_x - 1, _y);

			if(currentCell->HasWall(Wall::WEST, true) == false)
				nextCells.push_back(westCell);
		}

		std::sort(nextCells.begin(), nextCells.end(), SortFloodfillCells);

		for(Cell* cell : nextCells)
		{
			cell->GetFloodfill();
		}

		Cell* finalNextCell = NULL;
		if(nextCells.size() > 0)
			finalNextCell = nextCells[0];

		if(finalNextCell != NULL)
		{
			if(IsRotatedTowards(finalNextCell) == false)
				RotateTowards(finalNextCell);
			else
			{
				_x = finalNextCell->GetPositionX();
				_y = finalNextCell->GetPositionY();
			}
		}
	}
}

void Mouse::SetState(State state)
{
	_state = state;
}

State Mouse::GetState()
{
	return _state;
}

void Mouse::SetTarget(Target target)
{
	_target = target;
}

Target Mouse::GetTarget()
{
	return _target;
}

bool Mouse::IsRotatedTowards(Cell* cell)
{
	if((_rotation <= 45 || _rotation >= 315) && cell->GetPositionY() - _y > 0)
		return true;

	if(_rotation >= 45 && _rotation <= 135 && cell->GetPositionX() - _x < 0)
		return true;

	if(_rotation >= 135 && _rotation <= 225 && cell->GetPositionY() - _y < 0)
		return true;

	if(_rotation >= 225 && _rotation <= 315 && cell->GetPositionX() - _x > 0)
		return true;

	return false;
}

void Mouse::RotateTowards(Cell* cell)
{
	if(_x - cell->GetPositionX() > 0)
		_rotation = 90;
	else if(_x - cell->GetPositionX() < 0)
		_rotation = 270;
	else if(_y - cell->GetPositionY() > 0)
		_rotation = 180;
	else if(_y - cell->GetPositionY() < 0)
		_rotation = 0;
}

void Mouse::Reset()
{
	_x = 0;
	_y = 0;
	_rotation = 0;
	_state = State::IDLE;
	_target = Target::NONE;
	_targetCells = std::vector<Cell*>();
}

std::vector<Cell*> Mouse::GetTargetCells()
{
	return _targetCells;
}

void Mouse::SetTargetCells(std::vector<Cell*> cells)
{
	_targetCells.clear();

	for(Cell* cell : cells)
		_targetCells.push_back(cell);
}

#endif
