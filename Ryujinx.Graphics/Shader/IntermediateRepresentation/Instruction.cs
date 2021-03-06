using System;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    [Flags]
    enum Instruction
    {
        Absolute = 1,
        Add,
        BitfieldExtractS32,
        BitfieldExtractU32,
        BitfieldInsert,
        BitfieldReverse,
        BitwiseAnd,
        BitwiseExclusiveOr,
        BitwiseNot,
        BitwiseOr,
        Branch,
        BranchIfFalse,
        BranchIfTrue,
        Ceiling,
        Clamp,
        ClampU32,
        CompareEqual,
        CompareGreater,
        CompareGreaterOrEqual,
        CompareGreaterOrEqualU32,
        CompareGreaterU32,
        CompareLess,
        CompareLessOrEqual,
        CompareLessOrEqualU32,
        CompareLessU32,
        CompareNotEqual,
        ConditionalSelect,
        ConvertFPToS32,
        ConvertS32ToFP,
        ConvertU32ToFP,
        Copy,
        Cosine,
        Discard,
        Divide,
        EmitVertex,
        EndPrimitive,
        ExponentB2,
        Floor,
        FusedMultiplyAdd,
        IsNan,
        LoadConstant,
        LoadGlobal,
        LoadLocal,
        LogarithmB2,
        LogicalAnd,
        LogicalExclusiveOr,
        LogicalNot,
        LogicalOr,
        LoopBreak,
        LoopContinue,
        MarkLabel,
        Maximum,
        MaximumU32,
        Minimum,
        MinimumU32,
        Multiply,
        Negate,
        PackDouble2x32,
        PackHalf2x16,
        ReciprocalSquareRoot,
        Return,
        ShiftLeft,
        ShiftRightS32,
        ShiftRightU32,
        Sine,
        SquareRoot,
        StoreGlobal,
        StoreLocal,
        Subtract,
        TextureSample,
        TextureSize,
        Truncate,
        UnpackDouble2x32,
        UnpackHalf2x16,

        Count,
        FP   = 1 << 16,
        Mask = 0xffff
    }
}