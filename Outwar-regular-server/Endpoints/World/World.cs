﻿namespace Outwar_regular_server.Endpoints;

public class World
{
    // [x,y]
    // This is diamond city map replica
    public static int [,] gameMap = new int[20, 45]
    {
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  8, 14, 14, 14, 14, 14, 14, 14,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  9,  8,  0,  0,  0, 15,  0,  0, 16,  0, 35,  0,  0,  0,  0, 39, 41,  0,  0,  0,  0,  0,  0, 53, 50,  0,  0,  0,  0, 58,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  8,  0,  0,  0,  0,  0, 17, 16,  0, 38, 38, 38,  0,  0,  0, 41,  0,  0,  0, 45, 46,  0,  0, 50, 52,  0, 56, 57, 57,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  2,  0,  0,  0,  0,  0,  0,  0,  8,  0,  0,  0,  0,  0,  0, 16,  0, 33,  0, 40, 43,  0,  0, 41,  0,  0,  0, 45,  0,  0, 51, 50,  0, 59, 56,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  1,  1,  1,  1,  4,  4,  4,  6,  6,  7,  6,  6,  6,  6,  6,  6, 16,  0, 33,  0, 42, 42, 42, 42, 41,  0,  0,  0, 45,  0,  0,  0, 50,  0,  0, 56,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  3,  0,  0,  0,  0,  0,  5,  8,  0,  0, 19,  0,  0,  0, 16,  0, 33,  0,  0,  0,  0,  0, 41,  0,  0, 47, 45,  0,  0,  0, 50, 54, 54, 56, 60, 60,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  8,  0,  0,  0,  0,  0, 18, 16,  0, 33, 36, 36, 36, 37,  0, 41,  0,  0,  0, 45,  0,  0, 49, 50, 55,  0, 56,  0, 63, 63, 63, 63,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0, 10,  8, 20,  0, 22,  0,  0,  0, 16,  0, 33,  0,  0,  0,  0,  0, 41, 44, 44, 44, 45, 48, 48, 48, 48,  0,  0, 56,  0, 62,  0,  0, 63,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  8, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 56, 61, 61,  0,  0, 63,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  8,  0, 21,  0, 23,  0,  0,  0, 32,  0,  0,  0, 31,  0,  0, 41,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 63,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  8,  0,  0,  0, 23,  0,  0,  0,  0,  0,  0,  0, 31,  0,  0, 41,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 63,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  8,  0,  0,  0, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24,  0,  0,  0,  0,  0,  0,  0,  0, 65, 64, 64, 63, 63, 63, 63,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  8,  0,  0,  0,  0,  0,  0, 25,  0,  0,  0,  0,  0, 30,  0, 29,  0,  0,  0,  0,  0, 71,  0,  0,  0, 65,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  8,  8,  8,  0,  0,  0,  0,  0,  0, 25,  0,  0, 28,  0,  0,  0,  0, 29,  0,  0,  0,  0,  0, 70,  0,  0,  0, 65,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0, 13,  0, 12,  0,  0,  0,  0,  0,  0, 25, 27, 27, 27, 27, 27, 27, 27, 27,  0,  0,  0,  0,  0, 70,  0,  0,  0, 65, 67, 67,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 25,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 72,  0, 70,  0,  0, 69, 65,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 25,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 68, 68, 68, 68, 68, 68, 68, 65,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 26, 25, 25,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 73,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 74,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0  },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0 }
    };
}