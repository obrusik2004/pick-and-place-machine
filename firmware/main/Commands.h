#ifndef COMMANDS_H
#define COMMANDS_H

#include "Config.h"

class CommandParser {
public:
    void parseCommand(String input);
    void liveMonitor();
};

extern CommandParser commandParser;

#endif