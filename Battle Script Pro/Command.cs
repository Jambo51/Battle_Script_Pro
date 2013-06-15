using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle_Script_Pro
{
    public class Command
    {
        private ushort hexID;
        private byte numberOfParameters;
        private string[] paramNames;
        private byte[] paramLengths;

        public ushort HexID
        {
            get { return hexID; }
        }

        public byte NumberOfParameters
        {
            get { return numberOfParameters; }
        }

        public string[] ParameterNames
        {
            get { return paramNames; }
        }

        public byte[] ParameterLengths
        {
            get { return paramLengths; }
        }

        public Command(ushort theHexID)
        {
            hexID = theHexID;
        }

        public Command(ushort theHexID, byte theNumberOfParameters)
            : this(theHexID)
        {
            numberOfParameters = theNumberOfParameters;
        }

        public Command(ushort theHexID, byte theNumberOfParameters, string[] theParameterNames, byte[] theParameterLengths)
            : this(theHexID, theNumberOfParameters)
        {
            paramNames = theParameterNames;
            paramLengths = theParameterLengths;
        }
    }
}
