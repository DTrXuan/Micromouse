#ifndef OLED_H
#define OLED_H

class Oled
{

private:
	/* Here will be the instance stored. */
	static Oled* instance;

	/* Private constructor to prevent instancing. */
	Oled();

public:
	enum class Page
	{
		Debug,
		Logo,
		Maze,
		Settings,
	};

    /* Static access method. */
	static Oled* Instance();

	void Draw(Page page);
	void Setup();
};

#endif
