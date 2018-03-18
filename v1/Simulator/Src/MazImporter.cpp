#include "MazImporter.hpp"
#include "Maze.hpp"
#include "Config.hpp"
#include "Utils.hpp"

#include <fstream>
#include <string>
#include <windows.h>
#include <iostream>
#include "dirent.h"

const std::string basePath = Utils::GetCurrentPath().append("mazes\\");

std::vector<std::string> ListMazeFiles() {
	std::vector<std::string> mazeFiles; // Empty on creation

	DIR *dir;
	struct dirent *ent;
	if ((dir = opendir(basePath.c_str())) != NULL) {
		while ((ent = readdir(dir)) != NULL) {
			mazeFiles.push_back(ent->d_name);
		}
		closedir(dir);
	}

	return mazeFiles;
}

bool ImportMaze(std::string filename) {
	std::string path = basePath;
	path.append(filename);

	std::ifstream mazeFile(path, std::ios::in);
	if (!mazeFile.is_open())
		return false;

	// get length of file
	mazeFile.seekg(0, mazeFile.end);
	size_t length = mazeFile.tellg();
	mazeFile.seekg(0, mazeFile.beg);

	char buffer[length];

	mazeFile.read(buffer, length);

	for (int y = 0; y < Config::Height; y++) {
		for (int x = 0; x < Config::Width; x++) {
			Maze::Instance()->GetCell(x, y)->SetWall((Wall) buffer[y + x * 16], false);
		}
	}

	return true;
}

void ExportMaze(std::string filename)
{
	std::ofstream saveFile;

	std::string filePath(filename);
	std::string fileType(".maz");

	if(filePath.size() >= fileType.size() && filePath.compare(filePath.size() - fileType.size(), fileType.size(), fileType) != 0)
		filePath.append(fileType);

	saveFile.open(filePath);

	for(int y = 0; y < Config::Height; y++)
	{
		for(int x = 0; x < Config::Width; x++)
		{
			Cell* cell = Maze::Instance()->GetCell(x, y);
			int value = (int) cell->GetWalls(false);
			saveFile.write((char*)&value, sizeof(char));
		}
	}
}
