#include <libas.h>

#define ClockPin PD7
#define ChipSelectPin PD5
#define DataPin PD3

libas Encoder(ClockPin, DataPin, ChipSelectPin);

// the setup function runs once when you press reset or power the board
void setup() {
	Serial.begin(9600);
}

bool on = true;
int count = 0;

// the loop function runs over and over again until power down or reset
void loop()
{
	/**/
	Serial.print("READ # "); Serial.println(count++);
	int value = Encoder.GetPosition();
	Serial.println(value);
	/** /
	ASDataFlags flags = Encoder.Flags;
	Serial.println(flags.data);
	Serial.println(flags.OCF);
	Serial.println(flags.COF);
	Serial.println(flags.LIN);
	Serial.println(flags.MagInc);
	Serial.println(flags.MagDec);
	Serial.println(flags.EvenPar);
	/**/
	Serial.println("---------------------");
	/** /
	digitalWrite(ChipSelectPin, on ? HIGH : LOW);
	on = !on;
	/**/
	delay(250);
}