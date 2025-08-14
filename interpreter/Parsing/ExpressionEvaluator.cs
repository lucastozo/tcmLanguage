using interpreter.Utils;

namespace interpreter.Parsing
{
    public static class ExpressionEvaluator
    {
        public static byte EvaluateExpression(string expression, ParserContext context, 
                                            int lineNumber, bool allowOverflow = false)
        {
            try
            {
                if (ContainsOperators(expression))
                {
                    return (byte)ParseExpression(expression, context, allowOverflow);
                }
    
                if (expression.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                {
                    string binaryPart = expression.Substring(2);
                    if (binaryPart.Length != 8)
                    {
                        throw new Exception($"Binary value must have exactly 8 bits, got {binaryPart.Length} at line {lineNumber}");
                    }
                    if (!binaryPart.All(c => c == '0' || c == '1'))
                    {
                        throw new Exception($"Binary value can only contain 0 and 1 at line {lineNumber}");
                    }
                    return Convert.ToByte(binaryPart, 2);
                }
    
                if (expression.All(char.IsDigit))
                {
                    int v = int.Parse(expression);
                    
                    if (allowOverflow)
                    {
                        if (v < 0)
                        {
                            while (v < 0) v += byte.MaxValue+1;
                        }
                        return (byte)(v % (byte.MaxValue+1));
                    }
                    else
                    {
                        if (v < byte.MinValue || v > byte.MaxValue) throw new Exception($"Value {v} out of range ({byte.MinValue}-{byte.MaxValue}) at line {lineNumber}");
                        return (byte)v;
                    }
                }
    
                if (context.Constants.TryGetValue(expression, out byte constVal))
                {
                    return constVal;
                }
    
                if (context.Labels.TryGetValue(expression, out int labelAddr))
                {
                    return (byte)labelAddr;
                }
    
                if (context.Subroutines.TryGetValue(expression, out int subroutineAddr))
                {
                    return (byte)subroutineAddr;
                }
    
                if (Keywords.list.TryGetValue(expression, out byte kwVal))
                {
                    return kwVal;
                }
    
                throw new Exception($"Unknown token '{expression}' at line {lineNumber}");
            }
            catch (Exception ex) when (!(ex.Message.StartsWith("Unknown token") || ex.Message.StartsWith("Value")))
            {
                throw new Exception($"Error evaluating expression '{expression}' at line {lineNumber}: {ex.Message}");
            }
        }
    
        private static bool ContainsOperators(string expression)
        {
            return expression.Contains('|') || expression.Contains('+') || expression.Contains('-') ||
                   expression.Contains('*') || expression.Contains('/') || expression.Contains('%') ||
                   expression.Contains('^') || expression.Contains('&');
        }
    
        private static int ParseExpression(string expression, ParserContext context, bool allowOverflow = false)
        {
            // Handle operators
            // 1. Multiplication (*), Division (/), Modulo (%)
            // 2. Addition (+), Subtraction (-)
            // 3. Bitwise operations (|, ^, &)
    
            if (expression.Contains('|'))
            {
                return ParseBinaryOperation(expression, '|', context, (a, b) => a | b, allowOverflow);
            }
            if (expression.Contains('^'))
            {
                return ParseBinaryOperation(expression, '^', context, (a, b) => a ^ b, allowOverflow);
            }
            if (expression.Contains('&'))
            {
                return ParseBinaryOperation(expression, '&', context, (a, b) => a & b, allowOverflow);
            }
    
            if (expression.Contains('+'))
            {
                return ParseBinaryOperation(expression, '+', context, (a, b) => a + b, allowOverflow);
            }
            if (expression.Contains('-'))
            {
                return ParseBinaryOperation(expression, '-', context, (a, b) => a - b, allowOverflow);
            }
    
            if (expression.Contains('*'))
            {
                return ParseBinaryOperation(expression, '*', context, (a, b) => a * b, allowOverflow);
            }
            if (expression.Contains('/'))
            {
                return ParseBinaryOperation(expression, '/', context, (a, b) =>
                {
                    if (b == 0) throw new Exception("Division by zero");
                    return a / b;
                }, allowOverflow);
            }
            if (expression.Contains('%'))
            {
                return ParseBinaryOperation(expression, '%', context, (a, b) =>
                {
                    if (b == 0) throw new Exception("Modulo by zero");
                    return a % b;
                }, allowOverflow);
            }
    
            throw new Exception($"Invalid expression '{expression}'");
        }
    
        private static int ParseBinaryOperation(string expression, char op, ParserContext context,
                                              Func<int, int, int> operation, bool allowOverflow = false)
        {
            // Find the rightmost occurrence of the operator to handle left-to-right evaluation
            int opIndex = expression.LastIndexOf(op);
            if (opIndex == -1) return ParseExpression(expression, context, allowOverflow);
    
            string left = expression.Substring(0, opIndex).Trim();
            string right = expression.Substring(opIndex + 1).Trim();
    
            int leftVal = EvaluateExpression(left, context, 0, allowOverflow);
            int rightVal = EvaluateExpression(right, context, 0, allowOverflow);
    
            int result = operation(leftVal, rightVal);
    
            if (allowOverflow)
            {
                if (result < 0)
                {
                    while (result < 0) result += 256;
                }
                return result % 256;
            }
            else
            {
                if (result < byte.MinValue || result > byte.MaxValue)
                {
                    throw new Exception($"Expression result {result} out of range ({byte.MinValue}-{byte.MaxValue})");
                }
                return result;
            }
        }
    }
}