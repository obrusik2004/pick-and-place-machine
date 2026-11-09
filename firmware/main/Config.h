#ifndef CONFIG_H
#define CONFIG_H
#include <AccelStepper.h>
// Pin Definitions
struct Pins {
  const int dirY = 2;
  const int dirX = 30;
  const int pulseX = 22;
  const int enableX = 3;
  const int pulseY = 8;
  const int enableY = 23;
  const int dirZ = 46;
  const int pulseZ = 38;
  const int enableZ = 9;
  const int enableRot = 50;
  const int pulseRot = 34;
  const int dirRot = 42;
  const int pump = 35;
  
  //==========================
   const int feederStep = 11;    // Pulse pin
  const int feederDir = 5;      // Direction pin  
  const int feederEnable = 25;  // Enable pin
  //===============
  const int limitY1 = A1;
  const int limitY2 = A4;
  const int limitX1 = A2;
  const int limitX2 = A3;
  const int limitZ1 = A0;
};

// Steps per MM
extern const float STEPS_PER_MM_X;
extern const float STEPS_PER_MM_Y; 
extern const float STEPS_PER_MM_Z;
extern const float STEPS_PER_DEG_R;

// Global State Structures
struct Position {
  long X = 0;
  long Y = 0;
  long Z = 0;
  long R = 0;
};

struct PCBReference {
  long X = 0;
  long Y = 0;
  long Z = 0;
  long R = 0;
  bool isSet = false;
};

struct PickReference {
  long X = 0;
  long Y = 0;
  long Z = 0;
  long R = 0;
  bool isSet = false;
};

struct State {
  bool emergencyStop = false;
  bool isHomed = false;
  bool autoRecoveryInProgress = false;
  int lastXDirection = 0;
  int lastYDirection = 0; 
  int lastZDirection = 0;
};

struct TargetPosition {
  long X = 0;
  long Y = 0;
  long Z = 0;
  long R = 0;
  bool moving = false;
};

struct ProcedureState {
    bool active = false;
    int currentState = 0;
    bool componentPicked = false;
    float placeX, placeY, placeZ, placeR;
    bool feederInitialized = false;
};

// Global extern declarations
extern Pins pins;
extern Position currentPos;
extern PCBReference pcbRef;
extern PickReference pickRef;
extern State state;
extern TargetPosition targetPos;
extern ProcedureState procedureState;
extern AccelStepper stepperX, stepperY, stepperZ, stepperRot;
extern const int FEEDER_STEPS_PER_ADVANCE;

// Global function declarations
void HomingAxis();
void enableMotor(int enablePin);
void startProcedure();
void procedure();
void setPCBReference();
void setPickReference();
void pick();
void emergencyStopAll();
void immediateHardStop(AccelStepper &stepper, int enablePin);
void stopSingleAxis(char axis, int enablePin);

// Procedure-related function declarations
void handleProcedureApproval();
void sendPickCompleted();
void sendPlaceCompleted();
void sendAwaitingApproval();
void sendAwaitingCoordinates();
void abortProcedure();

//correction offset handling function declaration
void parseCorrectionOffsets(String command);

// Limit and movement function declarations
void checkLimitsDuringMove();
void recoverFromLimitX();
void recoverFromLimitY();
void recoverFromLimitZ();
void immediateAxisStop(AccelStepper &stepper, int enablePin);
long parseAxisValue(String input, char axis, int stepsPerMM);
int getNextAxisIndex(String input, int start);
void runMultiMove();

#endif