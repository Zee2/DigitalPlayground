using System.Collections.Generic;
using System.Collections;

namespace Utils {
    public enum LogicType{
        None = 0,
        NOT = 1 << 15,
        AND = 1 << 0,
        NAND = AND | NOT,
        OR = 1 << 1,
        NOR = OR | NOT,
        XOR = 1 << 2,
        XNOR = XOR | NOT,
        Lever = 1 << 3,
        Indicator = 1 << 4,
        Circuit = 1 << 5,

        Clock = 1 << 6,
        ShiftRegister4 = 1 << 7,

        ShiftRegister8 = 1 << 8,
        DFF = 1 << 9,

        FA1 = 1 << 10,

    }
}

