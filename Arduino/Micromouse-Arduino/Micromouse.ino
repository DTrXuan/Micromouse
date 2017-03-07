/******************************************************
https://www.arduino.cc/en/Reference/AttachInterrupt
https://www.arduino.cc/en/Tutorial/Graph
https://www.arduino.cc/en/Tutorial/Smoothing
https://www.arduino.cc/en/Tutorial/Calibration
*******************************************************/

#include <AccelStepper.h>
#include <MMkit.h>

#define IR_LEFT 3
#define IR_FRONT_L 2 
#define IR_FRONT_R 1 
#define IR_RIGHT 0

#define SWITCH_1 2
#define SWITCH_2 3

void EstablishConnectionToSerial();
void CalibrateSensors();
void ReadFromSerial();
void ReadSensors();
bool WillPause();
bool AlignFrontToWall();

AccelStepper motorLeft(AccelStepper::DRIVER, 9, 8);
AccelStepper motorRight(AccelStepper::DRIVER, 11, 10);

MMkit mouse(motorLeft, motorRight);

const int NUM_SENSORS = 4;
const int READS_WINDOW_SIZE = 5;
long countReads = 0;

const int ROTATION_THRESHOLD = 50;
const int ROTATION_FACTOR = 10;

int sensorValuesWindow[NUM_SENSORS][READS_WINDOW_SIZE];
int sensorValues[NUM_SENSORS];
int sensorsCalibrationMin[NUM_SENSORS];
int sensorsCalibrationMax[NUM_SENSORS];
const int DURATION_CALIBRATION_SECONDS = 5;

bool pause = false;
bool forcedPause = false;
const int DELAY_PAUSE_SECONDS = 3;

bool connectedToSerial = false;
const int CONNECTION_MAX_RETRIES = 5;
const int DELAY_CONNECTION = 500;

bool debugSensors = false;

void setup() {
	if(connectedToSerial)
	{
		Serial.begin(9600);
		while (!Serial);
		delay(1);
	}

	// while the serial stream is not open, do nothing

	Serial.println("setup() started...");

	EstablishConnectionToSerial();

	pinMode(SWITCH_1, INPUT);
	pinMode(SWITCH_2, INPUT);
	pinMode(LED_BUILTIN, OUTPUT);

	mouse.setupMMkit();
	delay(10);
	CalibrateSensors();

	Serial.println("setup() finished!");
}

void loop() {
	delay(10);

	ReadFromSerial();
	ReadSensors();

	if (WillPause()) {
		return;
	}

	if (pause) {
		pause = false;

		Serial.print("Resuming in ");
		Serial.print(DELAY_PAUSE_SECONDS);
		Serial.println("s...");
		delay(DELAY_PAUSE_SECONDS * 1000);

		if (WillPause()) {
			return;
		}

		Serial.println("Resume!");
	}

	RandomTest();

	return;

	mouse.run();

	if (AlignFrontToWall()) {
		mouse.run();
	}
	else {
		mouse.stop();
	}
}

bool WillPause() {
	if (digitalRead(SWITCH_1) || digitalRead(SWITCH_2) || forcedPause) {
		// pause if at least one of the switches is on
		if (!pause) {
			Serial.println("Pause!");
		}

		pause = true;
		return true;
	}

	return false;
}

void CalibrateSensors() {
	digitalWrite(LED_BUILTIN, HIGH);
	Serial.print("Calibrating sensors... ");
	Serial.print(DURATION_CALIBRATION_SECONDS);
	Serial.println("s");

	long startTime = millis();

	for (int i = 0; i < NUM_SENSORS; i++) {
		sensorsCalibrationMin[i] = 1023;
		sensorsCalibrationMax[i] = 0;
	}

	while (millis() - startTime < DURATION_CALIBRATION_SECONDS * 1000) {
		mouse.readIRSensors();
		for (int i = 0; i < NUM_SENSORS; i++) {
			int sensorValue = mouse.IRsensorsValues[i];
			sensorsCalibrationMin[i] = min(sensorsCalibrationMin[i], sensorValue);
			sensorsCalibrationMax[i] = max(sensorsCalibrationMax[i], sensorValue);
		}

		delay(10);
	}

	if (debugSensors) {
		for (int i = 0; i < NUM_SENSORS; i++) {
			Serial.print(sensorsCalibrationMin[i]);
			Serial.print(" ");
			Serial.print(sensorsCalibrationMax[i]);
			Serial.print(" | ");
		}
	
		Serial.println();
	}

	Serial.println("Sensors calibrated!");
	digitalWrite(LED_BUILTIN, LOW);
	delay(1000);
}

void ReadSensors() {
	mouse.readIRSensors();

	int readIndex = countReads % READS_WINDOW_SIZE;
	countReads++;
	int windowRealSize = min(countReads, READS_WINDOW_SIZE);

	for (int i = 0; i < NUM_SENSORS; i++) {
		sensorValues[i] = 0;
		sensorsCalibrationMin[i] = min(sensorsCalibrationMin[i], mouse.IRsensorsValues[i]);
		sensorsCalibrationMax[i] = max(sensorsCalibrationMax[i], mouse.IRsensorsValues[i]);

		sensorValuesWindow[i][readIndex] = map(mouse.IRsensorsValues[i], sensorsCalibrationMin[i], sensorsCalibrationMax[i], 0, 255);
		sensorValuesWindow[i][readIndex] = constrain(sensorValuesWindow[i][readIndex], 0, 255);

		for (int r = 0; r < windowRealSize; r++) {
			sensorValues[i] += sensorValuesWindow[i][r];
		}

		sensorValues[i] /= windowRealSize;

		if (debugSensors) {
			/** /
			Serial.print(mouse.IRsensorsValues[i]);
			Serial.print(" ");
			/**/
			Serial.print(sensorValues[i]);
			Serial.print(" | ");
		}
	}

	if (debugSensors) {
		Serial.println();
	}
}

void EstablishConnectionToSerial() {
	if(!connectedToSerial) {
		return;
	}

	Serial.print("Connecting to Serial... ");

	for (int i = 0; i < CONNECTION_MAX_RETRIES; i++) {
		if (Serial.available() <= 0) {
			Serial.print('#');
			delay(DELAY_CONNECTION);
		}
		else {
			Serial.println();
			Serial.println("Connection established!");
			connectedToSerial = true;
			return;
		}
	}

	Serial.println();
	Serial.println("Couldn't connect to Serial.");
}

void ReadFromSerial() {
	if (!connectedToSerial) {
		return;
	}

	int incomingBytes = Serial.available();
	if (incomingBytes <= 0) {
		return;
	}

	char command[50];

	int discardedBytes = Serial.readBytesUntil('>', command, incomingBytes);
	incomingBytes -= discardedBytes;

	if (incomingBytes <= 0) {
		return;
	}

	int commandLength = Serial.readBytesUntil('<', command, incomingBytes);
	Serial.print("Mouse received: ");
	Serial.println(command[0]);

	switch (command[0]) {
	case 'W':
		mouse.goForward(1);
		break;

	case 'S':
		mouse.goForward(-1);
		break;

	case 'A':
		break;

	case 'D':
		break;

	case ' ':
		mouse.stop();
		break;

	case 'P':
		forcedPause = !forcedPause;
		break;
	}
}

boolean AlignFrontToWall() {
	int deltaRotation = sensorValuesWindow[IR_FRONT_R] - sensorValuesWindow[IR_FRONT_L];

	if (abs(deltaRotation) > ROTATION_THRESHOLD) {
		mouse.rotate(deltaRotation / ROTATION_FACTOR);
		mouse.setForwardMotionSpeed(500);
		return true;
	}

	return false;
}

void RandomTest() {
	unsigned long startTime = millis();
	mouse.setForwardMotionSpeed(MMkit::cmToSteps(60));
	mouse.setForwardMotionAcceleration(MMkit::cmToSteps(120));
	mouse.goForward(100);
	mouse.run();

	forcedPause = true;
	Serial.print("Test took ");
	Serial.print(millis() - startTime);
	Serial.println("ms.");
}