#include "Emergency.h"
#include "Config.h"

EmergencyManager emergencyManager;

void EmergencyManager::clearEmergencyStop() {
    state.emergencyStop = false;
    state.autoRecoveryInProgress = false;
}
