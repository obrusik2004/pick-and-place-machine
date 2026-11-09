#include "Config.h"
#include <AccelStepper.h>

// Global variable definitions
const float STEPS_PER_MM_X = 26.57;
const float STEPS_PER_MM_Y =19.95;
const float STEPS_PER_MM_Z = 199.045;
const float STEPS_PER_DEG_R = 7.08333;

Pins pins;
Position currentPos;
PCBReference pcbRef;
PickReference pickRef;
State state;
TargetPosition targetPos;
ProcedureState procedureState;

