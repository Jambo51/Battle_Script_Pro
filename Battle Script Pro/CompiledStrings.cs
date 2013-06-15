using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle_Script_Pro
{
    public class CompiledStrings
    {
        private int[] table;
        private byte[] compiledStrings;

        public int[] Table
        {
            get { return table; }
            set { table = value; }
        }

        public byte[] CompiledStringsArray
        {
            get { return compiledStrings; }
        }

        public CompiledStrings(int[] theTable, byte[] theCompiledStrings)
        {
            table = theTable;
            compiledStrings = theCompiledStrings;
        }
    }
}
