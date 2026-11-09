#ifndef FEEDER_H
#define FEEDER_H

#include <Arduino.h>

class Feeder {
public:
    void initialize();
    void advanceTape();
    void nudgeForward();
    void feedSteps(int steps);

    // Sensor-related
    bool isBlack();           // simple "black or not" check (still available)
    void alignToBlack();      // advanced alignment: find darkest point
};

extern Feeder feeder;

#endif
