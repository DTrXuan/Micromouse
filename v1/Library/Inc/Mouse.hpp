#ifndef MOUSE_H
#define MOUSE_H

class Mouse
{
private:
	/* Here will be the instance stored. */
	static Mouse* instance;

	/* Private constructor to prevent instancing. */
	Mouse();


public:
    /* Static access method. */
	static Mouse* Instance();

	void Loop();
	void Setup();

	int GetBattery();
};

#endif
