/******************************************************
https://www.arduino.cc/en/Reference/AttachInterrupt
https://www.arduino.cc/en/Tutorial/Graph
https://www.arduino.cc/en/Tutorial/Smoothing
https://www.arduino.cc/en/Tutorial/Calibration
*******************************************************/

#include "src/Config.h"
#include "src/Micromouse.h"
#include "src/Mouse.h"
#include "src/WallSensors.h"
#include "src/SerialInterface.h"

bool Variables::pause = false;
bool Variables::forcedPause = false;

void setup()
{
	Serial.begin(9600);

	if (Constants::connectedToSerial)
	{
		// while the serial streams are not open, do nothing
		Serial1.begin(9600);
		while (!Serial1);
	}

	delay(100);

	Serial.println("setup() started...");

	pinMode(Pins::SWITCH_1, INPUT);
	pinMode(Pins::SWITCH_2, INPUT);
	pinMode(LED_BUILTIN, OUTPUT);
	// TODO(Pedro): Do we need to map the buzzer? What pin does it use?

	Mouse::Setup();
	WallSensors::Setup();

	delay(10);
	WallSensors::CalibrateSensors();

	//WallSensors::PrintSensorsInterval();

	Serial.println("setup() finished!");
}

void loop()
{
	if (Constants::connectedToSerial)
		SerialInterface::Read();

	//WallSensors::ReadSensorsRaw();
	//WallSensors::PrintSensorsRaw();
	
	WallSensors::ReadSensorsSmooth();
	//WallSensors::PrintSensorsSmooth();

	if (WillPause())
		return;

	if (Variables::pause)
		Resume();

	Test();

	return;
}

bool WillPause()
{
	if (digitalRead(Pins::SWITCH_1) || digitalRead(Pins::SWITCH_2) || Variables::forcedPause)
	{
		// pause if at least one of the switches is on
		if (!Variables::pause)
			Serial.println("Pause!");

		Variables::pause = true;
		return true;
	}

	return false;
}

void Resume()
{
	Variables::pause = false;

	Serial.print("Resuming in ");
	Serial.print(Constants::DELAY_PAUSE_SECONDS);
	Serial.println("s...");
	delay(Constants::DELAY_PAUSE_SECONDS * 1000);

	if (WillPause())
		return;

	Serial.println("Resume!");
}

void Test()
{
	Mouse::RunTest();
}