################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
CPP_SRCS += \
../Src/Cell.cpp \
../Src/Config.cpp \
../Src/Maze.cpp \
../Src/Mouse_Hardware.cpp \
../Src/Mouse_Software.cpp 

OBJS += \
./Src/Cell.o \
./Src/Config.o \
./Src/Maze.o \
./Src/Mouse_Hardware.o \
./Src/Mouse_Software.o 

CPP_DEPS += \
./Src/Cell.d \
./Src/Config.d \
./Src/Maze.d \
./Src/Mouse_Hardware.d \
./Src/Mouse_Software.d 


# Each subdirectory must supply rules for building sources it contributes
Src/%.o: ../Src/%.cpp
	@echo 'Building file: $<'
	@echo 'Invoking: Cross G++ Compiler'
	g++ -O0 -g3 -Wall -c -fmessage-length=0 -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


