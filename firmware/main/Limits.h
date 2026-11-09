#ifndef LIMITS_H
#define LIMITS_H

#include "Config.h"

class LimitManager {
public:
    void initializeLimitPins();
    void checkLimitsDuringMove();
    void recoverFromLimitX();
    void recoverFromLimitY();
    void recoverFromLimitZ();
    void immediateAxisStop(AccelStepper &stepper, int enablePin);
    
private:
    void processRecoveryWithSerial();
};

extern LimitManager limitManager;

#endif