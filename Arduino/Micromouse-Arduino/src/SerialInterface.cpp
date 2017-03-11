#include <Arduino.h>
#include "SerialInterface.h"
#include "Config.h"

void SerialInterface::Read()
{
	if (!Constants::connectedToSerial)
	{
		return;
	}

	int incomingBytes = Serial1.available();
	if (incomingBytes <= 0)
	{
		return;
	}

	char* command = (char*)malloc(incomingBytes * sizeof(byte));
	int readBytes = Serial1.readBytes(command, incomingBytes);

	Serial.print("Mouse received: ");
	Serial.println(command);

	switch (command[0])
	{
	case 'K':
		digitalWrite(LED_BUILTIN, LOW);
		break;

	case 'L':
		digitalWrite(LED_BUILTIN, HIGH);
		break;

	case 'P':
		Variables::forcedPause = !Variables::forcedPause;
		break;
	}

	free(command);
}