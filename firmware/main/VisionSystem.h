#ifndef VISION_SYSTEM_H
#define VISION_SYSTEM_H

#include "Config.h"

class VisionSystem {
public:
    void initialize();
    void handleVisionCommand(String command);
    void startVisionCheck();

    // keep public in case you call them elsewhere
    void processCorrectPlacement();
    void processIncorrectPlacement();

    bool isWaitingForFeedback() { return waitingForFeedback; }

private:
    bool waitingForFeedback = false;
};

extern VisionSystem visionSystem;

#endif
