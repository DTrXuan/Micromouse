////////////////////////////////////////////////////////////
// Headers
////////////////////////////////////////////////////////////
#include "iostream"
#include "fstream"
#include <ctime>
#include <windows.h>
#include "cmath"
#include "sstream"
#include <iomanip>

#include <TGUI/TGUI.hpp>
#include <SFML/OpenGL.hpp>

#include "MazImporter.hpp"
#include "Maze.hpp"
#include "Mouse.hpp"
#include "Config.hpp"
#include "PathFinder.hpp"
#include "Utils.hpp"

#define MAZE_LIST_ID 1

int windowWidth = 1000;
int windowHeight = 540;

int mazeWallLength = 32;
int mazeWallThickness = 4;
int mazeOffsetX = 10;
int mazeOffsetY = 10;

bool _renderFloodfill = false;
sf::RenderWindow* window;
tgui::Gui* gui;
tgui::ComboBox::Ptr mazeListComboBox;
tgui::Button::Ptr startFloodfillButton;
tgui::Button::Ptr saveMazeButton;

tgui::Label::Ptr simulationDelayLabel;
tgui::Slider::Ptr simulationDelaySlider;

tgui::Label::Ptr smoothCurvesLabel;
tgui::CheckBox::Ptr smoothCurvesCheckbox;

tgui::Label::Ptr heuristicCostsLabel;
tgui::Label::Ptr straightLineCostLabel;
tgui::Slider::Ptr straightLineCostSlider;
tgui::Label::Ptr diagonalLineCostLabel;
tgui::Slider::Ptr diagonalLineCostSlider;
tgui::Label::Ptr fullTurnCostLabel;
tgui::Slider::Ptr fullTurnCostSlider;

tgui::Theme::Ptr theme;

const sf::Color whiteColor(255, 255, 255);
const sf::Color redColor(255, 0, 0);
const sf::Color darkRedColor(48, 0, 0);
const sf::Color greenColor(0, 255, 0);
const sf::Color darkYellow(128, 128, 0);

sf::Font profontFont;
clock_t lastTime;

int simulationDelay = 500;
bool smoothCurves = true;
float straightLineCost = 15;
float diagonalLineCost = 10;
float fullTurnCost = 20;

void InitGUI();
void DrawMaze();
void DrawPaths();
void DrawMouse();
void DrawOled();
void MazeList_OnItemSelected(std::string selectedItem);
void StartFloodFill_OnPressed();
void SaveMaze_OnPressed();
void SimulationDelay_OnValueChanged();
void SmoothCurves_OnValueChanged();
void StraightLineCost_OnValueChanged();
void DiagonalLineCost_OnValueChanged();
void FullTurnCost_OnValueChanged();

void Update();
void Draw();

std::vector<Cell*> cellPath;
std::vector<std::pair<float, float>> coordinatePath;

////////////////////////////////////////////////////////////
/// Entry point of application
///
/// \return Application exit code
///
////////////////////////////////////////////////////////////
int main() {
	// Create the main window
	window = new sf::RenderWindow(sf::VideoMode(windowWidth, windowHeight), "R.A.T.0 v1",
			sf::Style::Default);

	gui = new tgui::Gui(*window);

	// Make it the active window for OpenGL calls
	window->setActive();

	// Set the color and depth clear values
	glClearDepth(1.f);
	glClearColor(0.f, 0.f, 0.f, 1.f);

	// Configure the viewport (the same size as the window)
	glViewport(0, 0, window->getSize().x, window->getSize().y);

	// Create a clock for measuring the time elapsed
	sf::Clock clock;

	InitGUI();

	// Start the game loop
	while (window->isOpen()) {
		// Process events
		sf::Event event;
		while (window->pollEvent(event)) {
			// Close window: exit
			if (event.type == sf::Event::Closed)
				window->close();

			// Escape key: exit
			if ((event.type == sf::Event::KeyPressed)
					&& (event.key.code == sf::Keyboard::Escape))
				window->close();

			// Resize event: adjust the viewport
			if (event.type == sf::Event::Resized)
				glViewport(0, 0, event.size.width, event.size.height);

			gui->handleEvent(event);
		}

		Update();
		Draw();
	}

	return EXIT_SUCCESS;
}

void Update() {
	clock_t currentTime = clock();

	if ((currentTime - lastTime) < simulationDelay)
		return;

	lastTime = currentTime;

	if (_renderFloodfill)
		Maze::Instance()->StepFloodfill();
}

void InitGUI() {
	profontFont.loadFromFile(Utils::GetCurrentPath().append("profont.ttf"));

	int leftPanelX = 540;
	int rightPanelX = 780;
	int leftHeight = 0;
	int rightHeight = 0;

	theme = tgui::Theme::create();
	mazeListComboBox = theme->load("ComboBox");
	mazeListComboBox->setSize(200, 20);
	mazeListComboBox->setPosition(leftPanelX, leftHeight += 10);
	mazeListComboBox->setScrollbar(theme->load("scrollbar"));
	mazeListComboBox->setItemsToDisplay(15);

	mazeListComboBox->connect("itemselected", MazeList_OnItemSelected);

	for (auto &mazeFile : ListMazeFiles())
		mazeListComboBox->addItem(mazeFile.c_str());

	gui->add(mazeListComboBox);

	startFloodfillButton = theme->load("Button");
	startFloodfillButton->setSize(200, 20);
	startFloodfillButton->setPosition(leftPanelX, leftHeight += 30);
	startFloodfillButton->setText("Start Floodfill");
	startFloodfillButton->connect("pressed", StartFloodFill_OnPressed);
	gui->add(startFloodfillButton);

	saveMazeButton = theme->load("Button");
	saveMazeButton->setSize(200, 20);
	saveMazeButton->setPosition(leftPanelX, leftHeight += 30);
	saveMazeButton->setText("Save Maze");
	saveMazeButton->connect("pressed", SaveMaze_OnPressed);
	gui->add(saveMazeButton);

	simulationDelayLabel = theme->load("Label");
	simulationDelayLabel->setSize(200, 20);
	simulationDelayLabel->setPosition(leftPanelX, leftHeight += 40);
	simulationDelayLabel->setTextColor(whiteColor);
	gui->add(simulationDelayLabel);

	simulationDelaySlider = theme->load("Slider");
	simulationDelaySlider->setSize(200, 20);
	simulationDelaySlider->setPosition(leftPanelX, leftHeight += 30);
	simulationDelaySlider->setMinimum(1);
	simulationDelaySlider->setMaximum(1000);
	simulationDelaySlider->setValue(simulationDelay);
	simulationDelaySlider->connect("valuechanged", SimulationDelay_OnValueChanged);
	SimulationDelay_OnValueChanged();
	gui->add(simulationDelaySlider);

	smoothCurvesCheckbox = theme->load("CheckBox");
	smoothCurvesCheckbox->setSize(30, 30);
	smoothCurvesCheckbox->setPosition(leftPanelX, leftHeight += 45);
	if(smoothCurves) smoothCurvesCheckbox->check();
	smoothCurvesCheckbox->connect("checked", SmoothCurves_OnValueChanged);
	smoothCurvesCheckbox->connect("unchecked", SmoothCurves_OnValueChanged);
	gui->add(smoothCurvesCheckbox);

	smoothCurvesLabel = theme->load("Label");
	smoothCurvesLabel->setSize(200, 20);
	smoothCurvesLabel->setPosition(leftPanelX + 40, leftHeight += 5);
	smoothCurvesLabel->setText("Smooth Curves");
	smoothCurvesLabel->setTextColor(whiteColor);
	gui->add(smoothCurvesLabel);

	heuristicCostsLabel = theme->load("Label");
	heuristicCostsLabel->setSize(200, 20);
	heuristicCostsLabel->setPosition(rightPanelX, rightHeight += 10);
	heuristicCostsLabel->setTextColor(whiteColor);
	heuristicCostsLabel->setText("---- Heuristic Costs ----");
	gui->add(heuristicCostsLabel);

	straightLineCostLabel = theme->load("Label");
	straightLineCostLabel->setSize(200, 20);
	straightLineCostLabel->setPosition(rightPanelX, rightHeight += 30);
	straightLineCostLabel->setTextColor(whiteColor);
	gui->add(straightLineCostLabel);

	straightLineCostSlider = theme->load("Slider");
	straightLineCostSlider->setSize(200, 20);
	straightLineCostSlider->setPosition(rightPanelX, rightHeight += 30);
	straightLineCostSlider->setMinimum(0);
	straightLineCostSlider->setMaximum(50);
	straightLineCostSlider->setValue(straightLineCost);
	straightLineCostSlider->connect("valuechanged", StraightLineCost_OnValueChanged);
	StraightLineCost_OnValueChanged();
	gui->add(straightLineCostSlider);

	diagonalLineCostLabel = theme->load("Label");
	diagonalLineCostLabel->setSize(200, 20);
	diagonalLineCostLabel->setPosition(rightPanelX, rightHeight += 30);
	diagonalLineCostLabel->setTextColor(whiteColor);
	gui->add(diagonalLineCostLabel);

	diagonalLineCostSlider = theme->load("Slider");
	diagonalLineCostSlider->setSize(200, 20);
	diagonalLineCostSlider->setPosition(rightPanelX, rightHeight += 30);
	diagonalLineCostSlider->setMinimum(0);
	diagonalLineCostSlider->setMaximum(50);
	diagonalLineCostSlider->setValue(diagonalLineCost);
	diagonalLineCostSlider->connect("valuechanged", DiagonalLineCost_OnValueChanged);
	DiagonalLineCost_OnValueChanged();
	gui->add(diagonalLineCostSlider);

	fullTurnCostLabel = theme->load("Label");
	fullTurnCostLabel->setSize(200, 20);
	fullTurnCostLabel->setPosition(rightPanelX, rightHeight += 30);
	fullTurnCostLabel->setTextColor(whiteColor);
	gui->add(fullTurnCostLabel);

	fullTurnCostSlider = theme->load("Slider");
	fullTurnCostSlider->setSize(200, 20);
	fullTurnCostSlider->setPosition(rightPanelX, rightHeight += 30);
	fullTurnCostSlider->setMinimum(0);
	fullTurnCostSlider->setMaximum(50);
	fullTurnCostSlider->setValue(fullTurnCost);
	fullTurnCostSlider->connect("valuechanged", FullTurnCost_OnValueChanged);
	FullTurnCost_OnValueChanged();
	gui->add(fullTurnCostSlider);
}

void Draw() {
	glClear(GL_COLOR_BUFFER_BIT);

	DrawMaze();
	DrawPaths();
	DrawMouse();

	DrawOled();

	gui->draw();
	window->display();
}

void DrawCells() {
	for (int y = 0; y < Config::Height; y++) {
		for (int x = 0; x < Config::Width; x++) {
			Cell* cell = Maze::Instance()->GetCell(x, y);

			if (cell->IsVisited()) {
				sf::RectangleShape ground(
						sf::Vector2f(mazeWallLength, mazeWallLength));
				ground.setPosition(mazeOffsetX + x * mazeWallLength,
						mazeOffsetY + (Config::Width - y - 1) * mazeWallLength);

				ground.setFillColor(darkRedColor);
				window->draw(ground);
			}

			if (_renderFloodfill) {
				std::stringstream stream;
				stream << cell->GetFloodfill();
				std::string floodfillStr = stream.str();

				sf::Text text;
				text.setCharacterSize(20);
				text.setFont(profontFont);
				text.setOrigin(floodfillStr.length() * text.getCharacterSize() / 4, text.getCharacterSize() / 2);
				text.setString(floodfillStr);
				text.setFillColor(redColor);
				text.setPosition(
						mazeOffsetX + x * mazeWallLength + mazeWallLength * 0.5f + text.getCharacterSize() / 4,
						mazeOffsetY + (Config::Width - y - 1) * mazeWallLength + mazeWallLength * 0.5f);

				window->draw(text);
			}
		}
	}
}

void DrawPosts() {
	for (int y = 0; y <= Config::Height; y++) {
		for (int x = 0; x <= Config::Width; x++) {
			sf::RectangleShape post(
					sf::Vector2f(mazeWallThickness, mazeWallThickness));
			post.setPosition(mazeOffsetX + x * mazeWallLength,
					mazeOffsetY + y * mazeWallLength);
			window->draw(post);
		}
	}
}

void DrawWalls() {
	for (int y = 0; y < Config::Height; y++) {
		for (int x = 0; x < Config::Width; x++) {
			Cell* cell = Maze::Instance()->GetCell(x, y);

			if (cell->HasWall(Wall::WEST, false)) {
				sf::RectangleShape wall(
						sf::Vector2f(mazeWallThickness,
								mazeWallLength - mazeWallThickness));
				wall.setPosition(mazeOffsetX + x * mazeWallLength,
						mazeOffsetY + mazeWallThickness
								+ (Config::Height - y - 1) * mazeWallLength);

				if (cell->HasWall(Wall::WEST, true))
					wall.setFillColor(redColor);

				window->draw(wall);
			}

			if (cell->HasWall(Wall::EAST, false)) {
				sf::RectangleShape wall(
						sf::Vector2f(mazeWallThickness,
								mazeWallLength - mazeWallThickness));
				wall.setPosition(mazeOffsetX + (x + 1) * mazeWallLength,
						mazeOffsetY + mazeWallThickness
								+ (Config::Height - y - 1) * mazeWallLength);

				if (cell->HasWall(Wall::EAST, true))
					wall.setFillColor(redColor);

				window->draw(wall);
			}

			if (cell->HasWall(Wall::SOUTH, false)) {
				sf::RectangleShape wall(
						sf::Vector2f(mazeWallLength - mazeWallThickness,
								mazeWallThickness));
				wall.setPosition(
						mazeOffsetX + mazeWallThickness + x * mazeWallLength,
						mazeOffsetY + (Config::Height - y) * mazeWallLength);

				if (cell->HasWall(Wall::SOUTH, true))
					wall.setFillColor(redColor);

				window->draw(wall);
			}

			if (cell->HasWall(Wall::NORTH, false)) {
				sf::RectangleShape wall(
						sf::Vector2f(mazeWallLength - mazeWallThickness,
								mazeWallThickness));
				wall.setPosition(
						mazeOffsetX + mazeWallThickness + x * mazeWallLength,
						mazeOffsetY
								+ (Config::Height - y - 1) * mazeWallLength);

				if (cell->HasWall(Wall::NORTH, true))
					wall.setFillColor(redColor);

				window->draw(wall);
			}
		}
	}
}

int vectorAngle(int x, int y) {
    if (x == 0) // special cases
        return (y > 0)? 90
            : (y == 0)? 0
            : 270;
    else if (y == 0) // special cases
        return (x >= 0)? 0
            : 180;
    int ret = atanf((float)y/x) * 180 / M_PI;
    if (x < 0 && y < 0) // quadrant
        ret = 180 + ret;
    else if (x < 0) // quadrant
        ret = 180 + ret; // it actually substracts
    else if (y < 0) // quadrant
        ret = 270 + (90 + ret); // it actually substracts
    return ret;
}

std::vector<std::pair<float, float>> ConvertCellPathToCoordinates(std::vector<Cell*> cellPath)
{
	std::vector<std::pair<float, float>> coordinates;

	if(cellPath.size() == 0)
		return coordinates;

	std::pair<float, float> lastCoord;

	for(int i = 0; i < cellPath.size(); i++)
	{
		Cell* cell = cellPath[i];

		std::pair<float, float> newCoord(cell->GetPositionX(), cell->GetPositionY());

		if(i == 0 || smoothCurves == false)
		{
			coordinates.push_back(newCoord);
		}
		else
		{
			float dx = newCoord.first - lastCoord.first;
			float dy = newCoord.second - lastCoord.second;

			float dxNorm = 0;
			float dyNorm = 0;

			if(dx != 0)
				dxNorm = dx / std::abs(dx);

			if(dy != 0)
				dyNorm = dy / std::abs(dy);

			newCoord.first -= dxNorm * 0.5f;
			newCoord.second -= dyNorm * 0.5f;

			coordinates.push_back(newCoord);

			newCoord = std::pair<float, float>(cell->GetPositionX(), cell->GetPositionY());
		}

		if(i == cellPath.size() - 1)
			coordinates.push_back(newCoord);

		lastCoord = newCoord;
	}

	return coordinates;
}

void DrawPaths()
{
	if(cellPath.size() == 0)
		return;

	coordinatePath = ConvertCellPathToCoordinates(cellPath);
	int pathThickness = 4;

	float lastX = 0;
	float lastY = 0;

	for(std::pair<float, float> coordinates : coordinatePath)
	{
		float newX = coordinates.first;
		float newY = coordinates.second;

		sf::Vector2f lastPoint(mazeOffsetX + lastX * mazeWallLength + mazeWallLength / 2 + pathThickness / 2, mazeOffsetY + (Config::Height - lastY - 1) * mazeWallLength + mazeWallLength / 2 + pathThickness + pathThickness / 2);
		sf::Vector2f newPoint(mazeOffsetX + newX * mazeWallLength + mazeWallLength / 2 + pathThickness / 2, mazeOffsetY + (Config::Height - newY - 1) * mazeWallLength + mazeWallLength / 2 + pathThickness + pathThickness / 2);

		sf::LineShape path(lastPoint, newPoint);
		path.setThickness(pathThickness);
		path.setOrigin(0, pathThickness);
		path.setFillColor(darkYellow);

		window->draw(path);

		lastX = newX;
		lastY = newY;
	}
}

void DrawMouse() {
	sf::ConvexShape mouse;

	mouse.setPointCount(3);

	mouse.setPoint(0, sf::Vector2f(5, 16));
	mouse.setPoint(1, sf::Vector2f(0, 0));
	mouse.setPoint(2, sf::Vector2f(10, 0));

	int xFinal = mazeOffsetX + mazeWallThickness / 2
			+ mazeWallLength * (Mouse::Instance()->GetPositionX() + 0.5f);
	int yFinal =
			mazeOffsetY + mazeWallThickness / 2
					+ mazeWallLength
							* (Config::Height
									- Mouse::Instance()->GetPositionY() - 0.5f);

	mouse.setOrigin(5, 8);
	mouse.setRotation(180);
	mouse.rotate(-Mouse::Instance()->GetRotation());
	mouse.setPosition(xFinal, yFinal);
	mouse.setFillColor(greenColor);
	window->draw(mouse);
}

void DrawMaze() {
	DrawCells();
	DrawPosts();
	DrawWalls();
}

int oledPosX = 540;
int oledPosY = 300;
float multiplier = 1;

void u8g_DrawStr(void* ignore, int xPos, int yPos, std::string string);

void DrawOled()
{
	int oledWidth = windowWidth - oledPosX - 15;
	int oledHeight = oledWidth / 2;
	multiplier = oledWidth / 128.f;

	sf::LineShape line(sf::Vector2f(oledPosX, oledPosY), sf::Vector2f(oledPosX + oledWidth, oledPosY));
	line.setThickness(mazeWallThickness);
	window->draw(line);

	line = sf::LineShape(sf::Vector2f(oledPosX, oledPosY), sf::Vector2f(oledPosX, oledPosY + oledHeight));
	line.setThickness(mazeWallThickness);
	window->draw(line);

	line = sf::LineShape(sf::Vector2f(oledPosX, oledPosY + oledHeight), sf::Vector2f(oledPosX + oledWidth, oledPosY + oledHeight));
	line.setThickness(mazeWallThickness);
	window->draw(line);

	line = sf::LineShape(sf::Vector2f(oledPosX + oledWidth, oledPosY), sf::Vector2f(oledPosX + oledWidth, oledPosY + oledHeight));
	line.setThickness(mazeWallThickness);
	window->draw(line);

	u8g_DrawStr(NULL, 2, 12, "Battery =");
	u8g_DrawStr(NULL,  50, 12, "100%");
}

void u8g_DrawStr(void* ignore, int xPos, int yPos, std::string string)
{
	int baseX = oledPosX + mazeWallThickness;
	int baseY = oledPosY + mazeWallThickness;
	sf::Text text;
	text.setFont(profontFont);
	text.setOrigin(0, 10 * multiplier);
	text.setString(string);
	text.setCharacterSize(10 * multiplier);
	text.setPosition(baseX + xPos * multiplier, baseY + yPos * multiplier);
	window->draw(text);
}

void CalculateCellPath()
{
	Maze* maze = Maze::Instance();
	SetHeuristicCosts(straightLineCost, diagonalLineCost, fullTurnCost);
	cellPath = FindPath(maze->GetCell(0, 0), maze->GetCell(7, 7), false);
}

void MazeList_OnItemSelected(std::string selectedItem) {
	_renderFloodfill = false;

	Maze* maze = Maze::Instance();
	maze->Reset(false);

	Mouse::Instance()->Reset();
	ImportMaze(selectedItem);

	CalculateCellPath();
}

void StartFloodFill_OnPressed() {
	_renderFloodfill = true;
	Mouse::Instance()->Reset();
	Maze::Instance()->ResetFloodfill();
}

void SaveMaze_OnPressed() {
	char filename[MAX_PATH];

	OPENFILENAME ofn;
	ZeroMemory(&filename, sizeof(filename));
	ZeroMemory(&ofn, sizeof(ofn));
	ofn.lStructSize = sizeof(ofn);
	ofn.hwndOwner = NULL;
	ofn.lpstrFilter = "MAZe file\0*.maz\0";
	ofn.lpstrFile = filename;
	ofn.nMaxFile = MAX_PATH;
	ofn.lpstrTitle = "Save maze as...";
	ofn.lpstrInitialDir = basePath.c_str();
	ofn.Flags = OFN_DONTADDTORECENT | OFN_FILEMUSTEXIST;

	if (GetSaveFileNameA(&ofn)) {
		ExportMaze(filename);
	}
}

void SimulationDelay_OnValueChanged()
{
	simulationDelay = simulationDelaySlider->getValue();
	simulationDelayLabel->setText(std::string("Delay: ").append(std::to_string(simulationDelay)));
}

void SmoothCurves_OnValueChanged()
{
	smoothCurves = smoothCurvesCheckbox->isChecked();

	CalculateCellPath();
}

void StraightLineCost_OnValueChanged()
{
	straightLineCost = straightLineCostSlider->getValue() / 10.f;

	std::stringstream stream;
	stream << std::fixed << std::setprecision(2) << straightLineCost;
	std::string straightLineCostStr = stream.str();

	straightLineCostLabel->setText(std::string("Straight Line: ").append(straightLineCostStr));

	CalculateCellPath();
}

void DiagonalLineCost_OnValueChanged()
{
	diagonalLineCost = diagonalLineCostSlider->getValue() / 10.f;

	std::stringstream stream;
	stream << std::fixed << std::setprecision(2) << diagonalLineCost;
	std::string diagonalLineCostStr = stream.str();

	diagonalLineCostLabel->setText(std::string("Diagonal Line: ").append(diagonalLineCostStr));

	CalculateCellPath();
}

void FullTurnCost_OnValueChanged()
{
	fullTurnCost = fullTurnCostSlider->getValue() / 10.f;

	std::stringstream stream;
	stream << std::fixed << std::setprecision(2) << fullTurnCost;
	std::string fullTurnCostStr = stream.str();

	fullTurnCostLabel->setText(std::string("Full Turn: ").append(fullTurnCostStr));

	CalculateCellPath();
}
