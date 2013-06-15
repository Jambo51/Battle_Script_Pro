using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle_Script_Pro
{
    public class DynamicPointer
    {
        private string name;
        private int location;
        private int lineNumber;
        private int parameterNumber;
        private int compiledPointerLocation;

        public string Name
        {
            get { return name; }
        }

        public int Location
        {
            get { return location; }
            set { location = value; }
        }

        public int CompiledPointerLocation
        {
            get { return compiledPointerLocation; }
            set { compiledPointerLocation = value; }
        }

        public int LineNumber
        {
            get { return lineNumber; }
        }

        public int ParameterNumber
        {
            get { return parameterNumber; }
        }

        public DynamicPointer(string theName, int theLineNumber, int theParameterNumber)
        {
            name = theName;
            lineNumber = theLineNumber;
            parameterNumber = theParameterNumber;
        }

        public DynamicPointer(string theName, int theLineNumber, int theParameterNumber, int theLocation, int theCompiledPointerLocation)
            : this(theName, theLineNumber, theParameterNumber)
        {
            location = theLocation;
            compiledPointerLocation = theCompiledPointerLocation;
        }
    }
}
