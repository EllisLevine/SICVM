﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SIC_Simulator
{

    // Assigned to Riley Strickland 
    // Assigned to Ellis Levine!
    class Instruction
    {
        public readonly string Symbol;
        public readonly string OpCode;
        public readonly string Operand;
        public readonly int LineNumber;
        public int MemoryAddress;
        public Instruction(String Symbol, String OpCode, String Operand, int LineNumber)
        {
            this.Symbol = Symbol;
            this.OpCode = OpCode;
            this.Operand = Operand;
            this.LineNumber = LineNumber;
        }
    }

    class Assembler
    {

        private static readonly char[] InvalidSymbolCharacters = { ' ', '$', '!', '=', '+', '-', '(', ')', '@' };

        public static bool IsDirective(string who) => Assembler.Directives.Contains(who);
        public static bool IsNotSymbol(string who)
        {
            if (String.IsNullOrEmpty(who))
                return false;
            if (who.Length > 6)
                return true;
            if (Char.IsDigit(who[0]))
                return true;
            return InvalidSymbolCharacters.Any(x => who.Contains(x)); //contains any bad characters? TODO: Stricter Checking as C# is UFT-16...
        }

        public static readonly Dictionary<string, int> Instructions = new Dictionary<string, int>
        {
            {"ADD", 0x18}, {"ADDF",0x58}, {"ADDR", 0x90}, {"AND", 0x40}, {"CLEAR", 0xB4}, {"COMP", 0x28}, {"COMPF", 0x88},
            {"COMPR", 0xA0}, {"DIV", 0x24}, {"DIVF", 0x64}, {"DIVR", 0x9C}, {"FIX", 0xC4}, {"FLOAT", 0xC0}, {"HIO", 0xC0},
            {"J", 0x3C}, {"JEQ", 0x30}, {"JGT", 0x34}, {"JLT", 0x38}, {"JSUB", 0x48}, {"LDA", 0x00}, {"LDB", 0x68}, {"LDCH", 0x50},
            {"LDF", 0x70}, {"LDL", 0x08}, {"LDS", 0x6C}, {"LDT", 0x74}, {"LDX", 0x04}, {"LPS", 0xD0}, {"MUL", 0x20}, {"MULF", 0x60},
            {"MULR", 0x98}, {"NORM",0xC8}, {"OR",0x44}, {"RD",0xD8}, {"RMO", 0xAC}, {"RSUB", 0x4C}, {"SHIFTL", 0xA4}, {"SHIFTR", 0xA8},
            {"SIO", 0xF0}, {"SSK", 0xEC}, {"STA", 0x0C}, {"STB", 0x78}, {"STCH", 0x54}, {"STF", 0x80}, {"STI", 0xD4},
            {"STL", 0x14}, {"STS", 0x7C}, {"STSW", 0xE8}, {"STT", 0x84}, {"STX", 0x10}, {"SUB", 0x1C}, {"SUBF", 0x5C},
            {"SUBR", 0x94},{"SVC", 0xB0}, {"TD", 0xE0}, {"TIO", 0xF8} ,{"TIX", 0x2C}, {"TIXR", 0xB8},{ "WD", 0xDC}
        };

        public static readonly HashSet<string> Directives = new HashSet<string> { "END", "BYTE", "WORD", "RESB", "RESW", "RESR", "EXPORTS", "START" };



        public string ObjectCode { get; private set; }
        public string SICSource { get; private set; } /* let's protect our variables from mutations */

        public Dictionary<string, Instruction> SymbolTable { get; private set; } = new Dictionary<string, Instruction>();

        private enum PROCESS { INIT, END, START, ERROR }
        PROCESS _process = PROCESS.INIT;
        public Assembler(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Input file {Path.GetFileNameWithoutExtension(filePath)} does not exist.");
                return; /* todo an error here */
            }

            /*
             ____________
            < BEGIN PASS I >
             ------------
                    \   ^__^
                     \  (oo)\_______
                        (__)\       )\/\
                            ||----w |
                            ||     ||
            */
            StreamReader file = new StreamReader(filePath);
            int memory_address = 0, line_counter = 0;

            String line;
            while ((line = file.ReadLine()) != null)
            {
                line = line.TrimEnd();
                line_counter++;
                if (line[0] == 35 || line.Length == 0) // skip comments and blank lines
                    continue;

                if (_process == PROCESS.END)
                {
                    // TODO throw END OF READ BUT STILL READING
                }

                String[] lineArray = line.Split('\t');

                Instruction instruction_line = new Instruction(lineArray[0], lineArray[1], lineArray.Length == 2 ? "" : lineArray[2], line_counter);

                if (instruction_line.Symbol.Length != 0)
                {
                    if (IsDirective(instruction_line.Symbol))
                    {
                        // TODO throw IS DIRECTIVE IN SYMBOL FIELD
                    }
                    if (IsNotSymbol(instruction_line.Symbol))
                    {
                        // TODO throw SYMBOL VALIDATION FAILD
                    }

                    if (SymbolTable.ContainsKey(instruction_line.Symbol))
                    {
                        // TODO throw DUPLICATE SYMBOL
                    }

                    if (instruction_line.OpCode.Equals("START"))
                    {
                        _process = PROCESS.START;

                        if (Int32.TryParse(instruction_line.Operand, System.Globalization.NumberStyles.HexNumber, null, out memory_address)) // check if hex value
                        {
                            instruction_line.MemoryAddress = memory_address;
                            if (instruction_line.MemoryAddress >= 32768)
                            {
                                //throwError(MEMORY_SIZE_);
                            }

                            SymbolTable.Add(instruction_line.Symbol, instruction_line);
                        }
                        else
                        {
                            //throwError(BYTE_HEX_FORMAT);
                        }

                        continue;
                    }
                    instruction_line.MemoryAddress = memory_address;
                    SymbolTable.Add(instruction_line.Symbol, instruction_line);
                }

                // DIDN'T FIND THE START DIRECTIVE ON THE FIRST PASS
                if (_process != PROCESS.START)
                {
                    //throwError(START_NOT_DEFINED);
                }

                instruction_line.MemoryAddress = memory_address;

                if (instruction_line.OpCode.Equals("END"))
                {
                    _process = PROCESS.END;
                    memory_address += 3; // not necessary ?
                    continue;
                }

                // START MEMORY INCREASE

                int len = 0; // var for numbers..
                if (Instructions.ContainsKey(instruction_line.OpCode))
                {
                    memory_address += 3;
                }
                else if (instruction_line.OpCode.Equals("WORD"))
                {

                    if (Int32.TryParse(instruction_line.Operand, out len))
                    {
                        //  throwError(WORD_FORMAT);
                    }

                    //if (len >= MAX_INT_SIZE || len <= MIN_INT_SIZE) // check max int size
                    //    throwError(WORD_SIZE_);

                    memory_address += 3;
                }
                else if (instruction_line.OpCode.Equals("RESW"))
                {
                    if (Int32.TryParse(instruction_line.Operand, out len))
                    {
                        //  throwError(WORD_FORMAT);
                    }

                    memory_address += 3 * len;
                }
                else if (instruction_line.OpCode.Equals("BYTE"))
                {
                    len = instruction_line.Operand.Length;
                    if (instruction_line.Operand[0] == 67)
                    { // char
                        //if (isNotByteDelimited(Operand, len))
                        //    throwError(BYTE_DELIMITER);
                        memory_address += (len - 2);
                    }
                    else if (instruction_line.Operand[0] == 88)
                    { // hex
                        //if (isNotByteDelimited(Operand, len))
                        //    throwError(BYTE_DELIMITER);

                        //if (!isHexLiteralStrRange(Operand, 2, len))
                        //    throwError(BYTE_HEX_FORMAT);

                        memory_address += (int)(len - 2) / 2;
                    }
                    else
                    {
                        //  throwError(BYTE_FLAG);
                    }
                }
                else if (instruction_line.OpCode.Equals("RESB"))
                {

                    if (Int32.TryParse(instruction_line.Operand, out len))
                    {
                        //  throwError(WORD_FORMAT);
                    }
                    memory_address += len;
                }
                else
                {
                    //throwError(GENERIC_ERROR);
                }

                if (memory_address >= 32768)
                {
                    //  throwError(WORD_SIZE_);
                }
            }

            String output ="";
            foreach(KeyValuePair<string, Instruction> tmp in SymbolTable){
                output += String.Format("{0}\t{1}\n", tmp.Value.Symbol, tmp.Value.MemoryAddress.ToString("X"));
            
            }
            MessageBox.Show(output);
            /*
             ____________
            < END PASS I >
             ------------
                    \   ^__^
                     \  (oo)\_______
                        (__)\       )\/\
                            ||----w |
                            ||     ||
            */
        }
    }
}


