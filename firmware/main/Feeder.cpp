#include "Feeder.h"
#include "Config.h"

// =============================
// FEEDER GLOBAL SETTINGS
// =============================
const int SENSOR_PIN = A6;


const int BLACK_THRESHOLD = 829;

int STEPS_PER_PITCH = 200;       // full pitch feed
const int NUDGE_STEPS = 35;      // small alignment nudges
const int SENSOR_SAMPLES = 50;   //feeder



// LOW-LEVEL ONE STEP FORWARD

void stepForward() {
    digitalWrite(pins.feederDir, LOW);
    digitalWrite(pins.feederStep, HIGH);
    delayMicroseconds(400);
    digitalWrite(pins.feederStep, LOW);
    delayMicroseconds(400);
}


// =============================
// READ SENSOR (averaged)
// =============================
int readSensorAvg() {
    long sum = 0;

    for (int i = 0; i < SENSOR_SAMPLES; i++) {
        sum += analogRead(SENSOR_PIN);
        delayMicroseconds(300);
    }

    return sum / SENSOR_SAMPLES;
}


// =============================
// CLASS INSTANCE
// =============================
Feeder feeder;


// =============================
// INITIALIZE FEEDER
// =============================
void Feeder::initialize() {
    pinMode(pins.feederStep, OUTPUT);
    pinMode(pins.feederDir, OUTPUT);
    pinMode(pins.feederEnable, OUTPUT);

    pinMode(SENSOR_PIN, INPUT);

    digitalWrite(pins.feederEnable, LOW);  // enable motor driver

    Serial.println("Feeder ready.");
}


// =============================
// MOVE STEPS
// =============================
void Feeder::feedSteps(int steps) {
    for (int i = 0; i < steps; i++) {
        stepForward();
    }

    Serial.print("FEED_STEPS: ");
    Serial.println(steps);
}


// =============================
// CHECK IF BLACK (TRUE) OR WHITE (FALSE)
// =============================
bool Feeder::isBlack() {
    int value = readSensorAvg();

    Serial.print("LDR = ");
    Serial.println(value);


    return (value <= BLACK_THRESHOLD);
}


// =============================
// ALIGN TO BLACK
// =============================
void Feeder::alignToBlack() {

    Serial.println("Aligning...");

    for (int attempt = 0; attempt < 60; attempt++) {

        delay(300);  // WAIT before reading to stabilize sensor
        int is_black_now = isBlack();

        if (is_black_now) {
            Serial.println("BLACK detected → STOP.");
            return;
        }

        Serial.println("WHITE → nudging...");
        feedSteps(NUDGE_STEPS);

        delayMicroseconds(800);  // WAIT again before next read
    }

    Serial.println("ERROR: Could not find black.");
}


// =============================
// MAIN ADVANCE FUNCTION
// =============================
void Feeder::advanceTape() {

    Serial.println("Feeding one pitch...");
    feedSteps(STEPS_PER_PITCH);

    delay(300);  // WAIT before reading the sensor after pitch

    alignToBlack();
}
