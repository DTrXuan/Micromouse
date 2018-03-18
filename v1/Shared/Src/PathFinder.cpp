#ifndef PATHFINDER_HPP
#define PATHFINDER_HPP

#include "PathFinder.hpp"

#include "Maze.hpp"

#include "queue"
#include "map"
#include "algorithm"

typedef std::pair<float, Cell*> Element;
std::priority_queue<Element, std::vector<Element>, std::greater<Element>> queue;

void put(Cell* item, float priority)
{
	queue.emplace(priority, item);
}

Cell* get()
{
	Cell* best_item = queue.top().second;
	queue.pop();
	return best_item;
}

float _straightLineCost = 1.5f;
float _fullTurnCost = 2.0f;
float _diagonalLineCost = 1.0f;

void SetHeuristicCosts(float straightLineCost, float diagonalLineCost, float fullTurnCost)
{
	_straightLineCost = straightLineCost;
	_diagonalLineCost = diagonalLineCost;
	_fullTurnCost= fullTurnCost;
}

float Heuristic(Cell* a, Cell* b)
{
	int dx = b->GetPositionX() - a->GetPositionX();
	int dy = b->GetPositionY() - a->GetPositionY();

	int cost = 0;

	cost = std::abs(dx) + std::abs(dy);

	return cost;
}

float CostToMove(Cell* beforePrevious, Cell* previous, Cell* current, Cell* next)
{
	int dxBP = previous->GetPositionX() - beforePrevious->GetPositionX();
	int dyBP = previous->GetPositionY() - beforePrevious->GetPositionY();

	int dxPC = current->GetPositionX() - previous->GetPositionX();
	int dyPC = current->GetPositionY() - previous->GetPositionY();

	int dxCN = next->GetPositionX() - current->GetPositionX();
	int dyCN = next->GetPositionY() - current->GetPositionY();

	if(dxPC == 0 && dyPC == 0) // from start cell, calculate PC --> CN
		return _straightLineCost;

	if(dxBP == dxCN && dyBP == dyCN)
		if(dxBP != dxPC && dyBP != dyPC)
			return _diagonalLineCost;

	if((dxBP == -dxCN && dxBP != 0) || (dyBP == -dyCN && dyBP != 0))
		return _fullTurnCost;

	return _straightLineCost;
}


std::vector<Cell*> FindPath(Cell* start, Cell* goal, bool detected)
{
	while(queue.size() > 0)
		queue.pop();

	std::vector<Cell*> path;
	std::map<Cell*, Cell*> came_from;
	std::map<Cell*, float> cost_so_far;

	Maze* maze = Maze::Instance();

	put(start, 0);

	came_from[start] = start;
	cost_so_far[start] = 0;

	while(queue.empty() == false)
	{
		Cell* current = get();

		if(current->IsEnd())
		{
			goal = current;
			break;
		}

		Cell* previous = came_from[current];
		Cell* beforePrevious = came_from[previous];
		for(Cell* next : maze->GetNeighbors(current, detected))
		{
			float new_cost = cost_so_far[current] + CostToMove(beforePrevious, previous, current, next);

			if(cost_so_far.find(next) == cost_so_far.end() || new_cost < cost_so_far[next])
			{
				cost_so_far[next] = new_cost;
				float priority = new_cost + Heuristic(next, goal);
				put(next, priority);
				came_from[next] = current;
			}
		}
	}


	Cell* current = goal;
	while (current != start)
	{
		path.push_back(current);
		current = came_from[current];
	}

	path.push_back(start);
	std::reverse(path.begin(), path.end());
	return path;

	return path;
}

#endif
