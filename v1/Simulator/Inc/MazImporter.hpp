#ifndef MAZIMPORTER_H
#define MAZIMPORTER_H

#include "string"
#include "vector"

extern const std::string basePath;
std::vector<std::string> ListMazeFiles();
bool ImportMaze(std::string filename);
void ExportMaze(std::string filename);

#endif
