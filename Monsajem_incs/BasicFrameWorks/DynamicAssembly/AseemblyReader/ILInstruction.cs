using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

namespace SDILReader
{
    public class ILInstruction
    {
        // Fields
        private OpCode code;
        private object operand;
        private byte[] operandData;
        private int offset;

        public Label? Label;
        public ILInstruction PointTo;

        // Properties
        public OpCode Code
        {
            get { return code; }
            set { code = value; }
        }

        public object Operand
        {
            get { return operand; }
            set { operand = value; }
        }

        public byte[] OperandData
        {
            get { return operandData; }
            set { operandData = value; }
        }

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        /// <summary>
        /// Returns a friendly strign representation of this instruction
        /// </summary>
        /// <returns></returns>
        public void IlEmiter(ILGenerator ilg)
        {
            if (Label != null)
                ilg.MarkLabel(Label.Value);

            if (operand == null)
                ilg.Emit(code);
            else

            {
                switch (code.OperandType)
                {
                    case OperandType.InlineField:
                        System.Reflection.FieldInfo fOperand = ((System.Reflection.FieldInfo)operand);
                        ilg.Emit(code, fOperand);
                        break;
                    case OperandType.InlineMethod:
                        var OprandType = operand.GetType();
                        ilg.Emit(code, (System.Reflection.MethodInfo)operand);                        
                        break;
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.InlineBrTarget:
                        ilg.Emit(code,PointTo.Label.Value);
                        break;
                    case OperandType.InlineType:
                        ilg.Emit(code,(Type)operand);
                        break;
                    case OperandType.InlineString:
                        ilg.Emit(code, (string)operand);
                        break;
                    case OperandType.ShortInlineVar:
                        ilg.Emit(code, (byte)operand);
                        break;
                    case OperandType.InlineI:
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineR:
                        ilg.Emit(code,(short)operand);
                        break;
                    case OperandType.InlineTok:
                        ilg.Emit(code, (Type)operand);
                        break;

                    default: throw new Exception("not supported");
                }
            }

        }

        /// <summary>
        /// Add enough zeros to a number as to be represented on 4 characters
        /// </summary>
        /// <param name="offset">
        /// The number that must be represented on 4 characters
        /// </param>
        /// <returns>
        /// </returns>
        private string GetExpandedOffset(long offset)
        {
            string result = offset.ToString();
            for (int i = 0; result.Length < 4; i++)
            {
                result = "0" + result;
            }
            return result;
        }

        public ILInstruction()
        {

        }
    }
}
