#ifndef STEPPERS_H
#define STEPPERS_H

#include "Config.h"

class StepperManager {
public:
    void initializeSteppers();
    void setTargetPosition(String input);
    void runMultiMove();
    void stopAllSteppers();
    long parseAxisValue(String input, char axis, float stepsPerMM);  // Change to float
    int getNextAxisIndex(String input, int start);

    bool isMoving();  

    // When true (set by Procedure), safety logic will NOT return Z
    // to originalZ after XY move; Z will stay at the safety height.
    bool procedureOverrideZReturn = false;

private:
    bool needsSafetyLift(long currentZ);
};

extern StepperManager stepperManager;

#endif
