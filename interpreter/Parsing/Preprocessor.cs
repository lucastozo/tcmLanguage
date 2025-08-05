using interpreter.Utils;

namespace interpreter.Parsing
{
    public static class Preprocessor
    {
        public static ParserContext ProcessFile(string[] lines, ParserSettings settings)
        {
            var context = new ParserContext();
            int instructionIndex = 0;

            context.SubroutineAddresses.Clear(); // Clear previous subroutine addresses
            
            var currentSettings = new ParserSettings { Overflow = settings.Overflow };

            // Preprocess
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int commentIdx = line.IndexOf('#');
                if (commentIdx >= 0) line = line[..commentIdx];
                line = string.Join(" ", line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(' ');

                 // Handle pragma directives
                if (PragmaProcessor.ProcessPragma(parts, i + 1, currentSettings))
                {
                    continue;
                }

                if (parts[0].Equals("const", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessConstant(parts, i + 1, context);
                    continue;
                }

                if (parts[0].Equals("label", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessLabel(parts, i + 1, context, instructionIndex);
                    continue;
                }

                if (parts[0].Equals("subroutine", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessSubroutine(parts, i + 1, context, instructionIndex);
                    continue;
                }

                if (instructionIndex >= 64)
                {
                    throw new Exception($"Program exceeds maximum of 64 instructions at line {i + 1}");
                }
                
                var settingsSnapshot = new ParserSettings { Overflow = currentSettings.Overflow };
                context.InstructionSettings.Add(settingsSnapshot);

                context.RawInstructionLines.Add(line);
                context.OriginalLineNumbers.Add(i + 1);
                Log.PrintMessage($"Line {line} preprocessed sucessfully");
                instructionIndex++;
            }

            return context;
        }
    
        private static void ProcessConstant(string[] parts, int lineNumber, ParserContext context)
        {
            if (parts.Length != 3) 
                throw new Exception($"Invalid const declaration at line {lineNumber}");
            
            if (!byte.TryParse(parts[2], out byte constVal))
            {
                throw new Exception($"Invalid const value at line {lineNumber}");
            }
    
            context.Constants[parts[1]] = constVal;
        }
    
        private static void ProcessLabel(string[] parts, int lineNumber, ParserContext context, int instructionIndex)
        {
            if (parts.Length != 2) 
                throw new Exception($"Invalid label at line {lineNumber}");
    
            /* 
            -- ABOUT LABEL AND INSTRUCTION INDEX * 4 --
                Turing Complete game treat label address as a single byte,
                instead of a index of instruction.
                So if subroutine is the 5th instruction, adress will be 4*4 = 16,
                because the 16th byte is the first byte of the 5th instruction.
                Kinda counterintuitive, but that's how the game works.
                This is why we multiply instruction index by 4.
            */
            if (instructionIndex * 4 > byte.MaxValue)
            {
                throw new Exception("Program exceeds maximum addressable memory of 256 bytes.");
            }
            
            context.Labels.Add(parts[1], instructionIndex * 4);
        }
    
        private static void ProcessSubroutine(string[] parts, int lineNumber, ParserContext context, int instructionIndex)
        {
            if (parts.Length != 2) 
                throw new Exception($"Invalid subroutine declaration at line {lineNumber}");
    
            if (instructionIndex * 4 > byte.MaxValue)
            {
                throw new Exception("Program exceeds maximum addressable memory of 256 bytes.");
            }
    
            int address = instructionIndex * 4;
            context.Subroutines.Add(parts[1], address);
            context.SubroutineAddresses.Add(address); // Store for interpreter access
            Log.PrintMessage($"[PARSER] Subroutine '{parts[1]}' registered at address {address}");
        }
    }
}