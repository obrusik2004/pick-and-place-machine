#ifndef HOMING_H
#define HOMING_H

#include "Config.h"

class HomingManager {
public:
    void homeAllAxes();
};

extern HomingManager homingManager;

#endif