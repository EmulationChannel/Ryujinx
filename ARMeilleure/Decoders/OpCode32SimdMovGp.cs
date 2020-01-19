﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ARMeilleure.Decoders
{
    class OpCode32SimdMovGp : OpCode32, IOpCode32Simd
    {
        public int Size => 2;

        public int Vn { get; private set; }
        public int Rt { get; private set; }
        public int Op { get; private set; }

        public int Opc1 { get; private set; }
        public int Opc2 { get; private set; }
        public OpCode32SimdMovGp(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            // which one is used is instruction dependant
            Op = ((opCode >> 20) & 0x1);

            Opc1 = ((opCode >> 21) & 0x3);
            Opc2 = ((opCode >> 5) & 0x3);

            Vn = ((opCode >> 7) & 0x1) | ((opCode >> 15) & 0x1e);
            Rt = (opCode >> 12) & 0xf;
        }
    }
}