#ifndef UTILS_H
#define UTILS_H
#include <algorithm>

class Utils
{
public:
	static long Map(long x, long in_min, long in_max, long out_min, long out_max)
	{
		long value = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

		value = std::max(value, out_min);
		value = std::min(value, out_max);

		return value;
	}
};

#endif
