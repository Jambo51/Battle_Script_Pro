using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle_Script_Pro
{
    public class SuperCommand
    {
        private byte numberOfParameters;
        private string[] paramNames;
        private byte[] paramLengths;

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

        public SuperCommand(byte theNumberOfParameters)
        {
            numberOfParameters = theNumberOfParameters;
        }

        public SuperCommand(byte theNumberOfParameters, string[] theParameterNames)
            : this(theNumberOfParameters)
        {
            paramNames = theParameterNames;
        }

        public SuperCommand(byte theNumberOfParameters, string[] theParameterNames, byte[] theParameterLengths)
            : this(theNumberOfParameters, theParameterNames)
        {
            paramLengths = theParameterLengths;
        }
    }
}
