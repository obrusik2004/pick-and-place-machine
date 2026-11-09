  #ifndef EMERGENCY_H
  #define EMERGENCY_H

  #include "Config.h"

  class EmergencyManager {
  public:
      void clearEmergencyStop();
  };

  extern EmergencyManager emergencyManager;

  #endif