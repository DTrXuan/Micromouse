#include "Maze.hpp"
#include "Config.hpp"
#include "Mouse.hpp"

#include "iostream"
#include "algorithm"
#include <iterator>
#include <queue>

Maze* Maze::instance = 0;

Maze* Maze::Instance()
{
    if (instance == 0)
    {
        instance = new Maze();
    }

    return instance;
}

Maze::Maze()
{
	Reset(false);
}


void Maze::Reset(bool detected)
{
	if(detected)
	{
		for(int y = 0; y < Config::Height; y++)
		{
			for(int x = 0; x < Config::Width; x++)
			{
				Cell* cell = GetCell(x, y);
				cell->Reset(true);
			}
		}

		return;
	}

	_cells = new Cell*[Config::Width * Config::Height * sizeof(Cell*)];

	for(int y = 0; y < Config::Height; y++)
	{
		for(int x = 0; x < Config::Width; x++)
		{
			int index = x + y * Config::Width;
			Cell* newCell = new Cell(x, y);

			if(x == 0)
				AddCellWall(newCell, Wall::WEST, false);

			if(x == Config::Width - 1)
				AddCellWall(newCell, Wall::EAST, false);

			if(y == 0)
				AddCellWall(newCell, Wall::SOUTH, false);

			if(y == Config::Height - 1)
				AddCellWall(newCell, Wall::NORTH, false);

			_cells[index] = newCell;
		}
	}
}

void Maze::AddCellWall(Cell* cell, Wall wall, bool detected)
{
	int x = cell->GetPositionX();
	int y = cell->GetPositionY();

	cell->AddWall(wall, detected);

	Cell* other;

	if(wall == Wall::EAST && x < Config::Width - 1)
	{
		other = _cells[(x+1) + y * Config::Width];
		other->AddWall(GetOppositeWall(wall), detected);
	}
	else if(wall == Wall::WEST && x > 0)
	{
		other = _cells[(x-1) + y * Config::Width];
		other->AddWall(GetOppositeWall(wall), detected);
	}
	else if(wall == Wall::NORTH && y < Config::Height - 1)
	{
		other = _cells[x + (y+1) * Config::Width];
		other->AddWall(GetOppositeWall(wall), detected);
	}
	else if(wall == Wall::SOUTH && y > 0)
	{
		other = _cells[x + (y-1) * Config::Width];
		other->AddWall(GetOppositeWall(wall), detected);
	}
}

void Maze::ResetFloodfill()
{
	for(int y = 0; y < Config::Height; y++)
	{
		for(int x = 0; x < Config::Width; x++)
		{
			int index = y * Config::Width + x;
			Cell* cell = _cells[index];
			cell->Reset(true);
			cell->SetFloodfill(GetDistanceToEnd(x, y));
		}
	}

	Mouse::Instance()->SetState(State::SEARCHING);
	Mouse::Instance()->SetTarget(Target::END);
}

void Maze::CalculateFloodFill(std::vector<Cell*> targetCells)
{
	std::queue<Cell*> nextStack;
	int currentFloodValue = 0;

	for(Cell* cell : targetCells)
		nextStack.push(cell);

	while(nextStack.size() > 0)
	{
		std::queue<Cell*> currentStack(nextStack);

		while(nextStack.size() > 0)
		{
			Cell* cell = nextStack.front();
			nextStack.pop();

			cell->SetQueued(true);
			cell->SetFloodfill(currentFloodValue);
		}

		while(currentStack.size() > 0)
		{
			Cell* cell = currentStack.front();
			currentStack.pop();

			Cell* leftCell = GetCell(cell->GetPositionX() - 1, cell->GetPositionY());
			Cell* rightCell = GetCell(cell->GetPositionX() + 1, cell->GetPositionY());
			Cell* upCell = GetCell(cell->GetPositionX(), cell->GetPositionY() + 1);
			Cell* downCell = GetCell(cell->GetPositionX(), cell->GetPositionY() - 1);

			if(leftCell != NULL && leftCell->GetQueued() == false && cell->HasWall(Wall::WEST, true) == false)
			{
				leftCell->SetQueued(true);
				nextStack.push(leftCell);
			}

			if(rightCell != NULL && rightCell->GetQueued() == false && cell->HasWall(Wall::EAST, true) == false)
			{
				rightCell->SetQueued(true);
				nextStack.push(rightCell);
			}

			if(downCell != NULL && downCell->GetQueued() == false && cell->HasWall(Wall::SOUTH, true) == false)
			{
				downCell->SetQueued(true);
				nextStack.push(downCell);
			}

			if(upCell != NULL && upCell->GetQueued() == false && cell->HasWall(Wall::NORTH, true) == false)
			{
				upCell->SetQueued(true);
				nextStack.push(upCell);
			}
		}

		currentFloodValue++;
	}
}

void Maze::StepFloodfill()
{
	Cell* currentCell = GetCell(Mouse::Instance()->GetPositionX(), Mouse::Instance()->GetPositionY());

	std::vector<Cell*> targetCells;

	if(currentCell->IsStart())
	{
		Mouse::Instance()->SetTarget(Target::END);

		for(int i = 0; i < Config::EndCellsLength; i++)
		{
			Cell* cell = Maze::Instance()->GetCell(Config::EndCells[i * 2], Config::EndCells[i * 2 + 1]);
			targetCells.push_back(cell);
		}
	}
	else if(currentCell->IsEnd())
	{
		// TODO(Pedro):
		Mouse::Instance()->SetTarget(Target::UNVISITED);

		Cell* cell;
		do
		{
			int x = rand() % Config::Width;
			int y = rand() % Config::Height;
			cell = Maze::Instance()->GetCell(x, y);
		}
		while(cell->IsVisited() || cell->IsSkipped() || cell->IsUnreachable());

		targetCells.push_back(cell);
	}
	else if(currentCell->IsTarget())
	{
		Mouse::Instance()->SetTarget(Target::START);
		Cell* cell = Maze::Instance()->GetCell(0, 0);
		targetCells.push_back(cell);
	}

	if(targetCells.size() > 0)
		Mouse::Instance()->SetTargetCells(targetCells);

	Mouse::Instance()->TriggerSensors();

	for(int i = 0; i < Config::Width * Config::Height; i++)
		_cells[i]->SetQueued(false);

	targetCells = Mouse::Instance()->GetTargetCells();
	CalculateFloodFill(targetCells);

	Mouse::Instance()->StepMotion();
}

int Maze::GetDistanceToEnd(int x, int y)
{
	int distance = 1000;

	for(int i = 0; i < Config::EndCellsLength; i++)
	{
		distance = std::min(distance, abs(x - Config::EndCells[i * 2]) + abs(y - Config::EndCells[i * 2 + 1]));
	}

	return distance;
}

Cell* Maze::GetCell(int x, int y)
{
	if(x < 0 || x >= Config::Width)
		return NULL;

	if(y < 0 || y >= Config::Height)
		return NULL;

	return _cells[x + y * Config::Width];
}

std::vector<Cell*> Maze::GetNeighbors(Cell* cell, bool detected)
{
	std::vector<Cell*> neighbors;

	if(cell->IsStart())
	{
		neighbors.push_back(GetCell(0, 1));
		return neighbors;
	}

	Cell* leftCell = GetCell(cell->GetPositionX() - 1, cell->GetPositionY());
	Cell* rightCell = GetCell(cell->GetPositionX() + 1, cell->GetPositionY());
	Cell* upCell = GetCell(cell->GetPositionX(), cell->GetPositionY() + 1);
	Cell* downCell = GetCell(cell->GetPositionX(), cell->GetPositionY() - 1);

	Cell* upLeftCell = GetCell(cell->GetPositionX() - 1, cell->GetPositionY() + 1);
	Cell* upRightCell = GetCell(cell->GetPositionX() + 1, cell->GetPositionY() + 1);
	Cell* downLeftCell = GetCell(cell->GetPositionX() - 1, cell->GetPositionY() - 1);
	Cell* downRightCell = GetCell(cell->GetPositionX() + 1, cell->GetPositionY() - 1);

	if(leftCell != NULL && cell->HasWall(Wall::WEST, detected) == false)
		neighbors.push_back(leftCell);

	if(rightCell != NULL && cell->HasWall(Wall::EAST, detected) == false)
		neighbors.push_back(rightCell);

	if(downCell != NULL && cell->HasWall(Wall::SOUTH, detected) == false)
		neighbors.push_back(downCell);

	if(upCell != NULL && cell->HasWall(Wall::NORTH, detected) == false)
		neighbors.push_back(upCell);

	return neighbors;
}
