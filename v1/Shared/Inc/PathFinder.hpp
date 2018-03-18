#ifndef PATHFINDER_H
#define PATHFINDER_H

#include "vector"

#include "Cell.hpp"

void SetHeuristicCosts(float straightLineCost, float diagonalLineCost, float fullTurnCost);
std::vector<Cell*> FindPath(Cell* start, Cell* goal, bool detected);

#endif
