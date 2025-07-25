class Parser
{
    public static List<Instruction> GetInstructions(string pathToFile)
    {
        Dictionary<string, byte> constants = new Dictionary<string, byte>();
        Dictionary<string, int> labels = new Dictionary<string, int>();
        List<string> rawInstructionLines = new List<string>();

        string[] lines = File.ReadAllLines(pathToFile);
        int instructionIndex = 0;

        // Preprocess
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            int commentIdx = line.IndexOf('#');
            if (commentIdx >= 0) line = line[..commentIdx];
            line = string.Join(" ", line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(' ');

            if (parts[0].Equals("const", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length != 3) throw new Exception($"Invalid const declaration at line {i + 1}");
                if (!byte.TryParse(parts[2], out byte constVal))
                {
                    throw new Exception($"Invalid const value at line {i + 1}");
                }

                constants[parts[1]] = constVal;
                continue;
            }

            if (parts[0].Equals("label", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length != 2) throw new Exception($"Invalid label at line {i + 1}");

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
                labels.Add(parts[1], instructionIndex * 4);
                
                continue;
            }

            rawInstructionLines.Add(line);
            instructionIndex++;
        }

        // Build instructions
        List<Instruction> instructions = new List<Instruction>();

        for (int i = 0; i < rawInstructionLines.Count; i++)
        {
            string[] parts = rawInstructionLines[i].Split(' ');
            List<byte> bParts = new();

            foreach (var part in parts)
            {
                if (part.All(char.IsDigit))
                {
                    int v = int.Parse(part);
                    if (v < 0 || v > 255) throw new Exception($"Value {v} out of byte range at instruction {i + 1}");
                    bParts.Add((byte)v);
                }
                else if (constants.TryGetValue(part, out byte constVal))
                {
                    bParts.Add(constVal);
                }
                else if (labels.TryGetValue(part, out int labelAddr))
                {
                    bParts.Add((byte)labelAddr);
                }
                else if (Keywords.list.TryGetValue(part, out byte kwVal))
                {
                    bParts.Add(kwVal);
                }
                else
                {
                    throw new Exception($"Unknown token '{part}' at instruction {i + 1}");
                }
            }

            if (bParts.Count != 4)
            {
                throw new Exception($"Invalid instruction at line {i + 1}: must have exactly 4 operands");
            }

            instructions.Add(new Instruction(bParts[0], bParts[1], bParts[2], bParts[3]));
        }

        return instructions;
    }
}