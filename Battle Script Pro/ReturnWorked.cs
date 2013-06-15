using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle_Script_Pro
{
    public class ReturnWorked
    {
        private bool worked;
        private uint intValueIfWorked;
        private ushort shortValueIfWorked;
        private byte byteValueIfWorked;
        private string nameOfPointer;
        private string generalReason;

        public bool Result
        {
            get { return worked; }
        }

        public uint Word
        {
            get { return intValueIfWorked; }
        }

        public ushort HalfWord
        {
            get { return shortValueIfWorked; }
        }

        public byte Byte
        {
            get { return byteValueIfWorked; }
        }

        public string PointerName
        {
            get { return nameOfPointer; }
        }

        public string GeneralReason
        {
            get { return generalReason; }
        }


        public ReturnWorked(bool theResult, byte theValue)
        {
            worked = theResult;
            byteValueIfWorked = theValue;
        }

        public ReturnWorked(bool theResult, ushort theValue)
        {
            worked = theResult;
            shortValueIfWorked = theValue;
        }

        public ReturnWorked(bool theResult, uint theValue)
        {
            worked = theResult;
            intValueIfWorked = theValue;
        }

        public ReturnWorked(bool theResult, string input, bool isPointer)
        {
            worked = theResult;
            if (isPointer)
            {
                nameOfPointer = input;
            }
            else
            {
                generalReason = input;
            }
        }
    }
}
