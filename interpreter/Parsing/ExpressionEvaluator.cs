using interpreter.Utils;

namespace interpreter.Parsing
{
    public static class ExpressionEvaluator
    {
        public static float EvaluateExpression(string expression, ParserContext context, 
                                            int lineNumber, bool allowOverflow = false)
        {
            try
            {
                if (ContainsOperators(expression))
                {
                    return ParseExpression(expression, context, allowOverflow, lineNumber);
                }
    
                if (float.TryParse(expression, System.Globalization.NumberStyles.Float, 
                           System.Globalization.CultureInfo.InvariantCulture, out float numValue))
                {
                    return numValue;
                }

                if (context.Macros.TryGetValue(expression, out string? macro))
                {
                    if (float.TryParse(macro, out float value))
                    {
                        return value;
                    }
                }
    
                if (context.Labels.TryGetValue(expression, out int labelAddr))
                {
                    return labelAddr;
                }
    
                if (context.Subroutines.TryGetValue(expression, out int subroutineAddr))
                {
                    return subroutineAddr;
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
            // Special case for negative numbers
            if (expression.StartsWith("-") && expression.Length > 1 && 
                !expression.Substring(1).Contains('+') && !expression.Substring(1).Contains('-') && 
                !expression.Substring(1).Contains('*') && !expression.Substring(1).Contains('/') && 
                !expression.Substring(1).Contains('%') && !expression.Substring(1).Contains('|') && 
                !expression.Substring(1).Contains('^') && !expression.Substring(1).Contains('&'))
            {
                return false;
            }
            
            return expression.Contains('|') || expression.Contains('+') || expression.Contains('-') ||
                   expression.Contains('*') || expression.Contains('/') || expression.Contains('%') ||
                   expression.Contains('^') || expression.Contains('&');
        }
    
        private static float ParseExpression(string expression, ParserContext context, bool allowOverflow = false, int lineNumber = 0)
        {
            // Handle operators
            // 1. Multiplication (*), Division (/), Modulo (%)
            // 2. Addition (+), Subtraction (-)
            // 3. Bitwise operations (|, ^, &)
    
            if (expression.Contains('|'))
            {
                return ParseBinaryOperation(expression, '|', context, (a, b) => (int)a | (int)b, allowOverflow, lineNumber);
            }
            if (expression.Contains('^'))
            {
                return ParseBinaryOperation(expression, '^', context, (a, b) => (int)a ^ (int)b, allowOverflow, lineNumber);
            }
            if (expression.Contains('&'))
            {
                return ParseBinaryOperation(expression, '&', context, (a, b) => (int)a & (int)b, allowOverflow, lineNumber);
            }
    
            if (expression.Contains('+'))
            {
                return ParseBinaryOperation(expression, '+', context, (a, b) => a + b, allowOverflow, lineNumber);
            }
            if (expression.Contains('-'))
            {
                return ParseBinaryOperation(expression, '-', context, (a, b) => a - b, allowOverflow, lineNumber);
            }
    
            if (expression.Contains('*'))
            {
                return ParseBinaryOperation(expression, '*', context, (a, b) => a * b, allowOverflow, lineNumber);
            }
            if (expression.Contains('/'))
            {
                return ParseBinaryOperation(expression, '/', context, (a, b) =>
                {
                    if (b == 0) throw new Exception("Division by zero");
                    return a / b;
                }, allowOverflow, lineNumber);
            }
            if (expression.Contains('%'))
            {
                return ParseBinaryOperation(expression, '%', context, (a, b) =>
                {
                    if (b == 0) throw new Exception("Modulo by zero");
                    return a % b;
                }, allowOverflow, lineNumber);
            }
    
            throw new Exception($"Invalid expression '{expression}'");
        }
    
        private static float ParseBinaryOperation(string expression, char op, ParserContext context,
                                              Func<float, float, float> operation, bool allowOverflow = false, int lineNumber = 0)
        {
            int opIndex = expression.IndexOf(op);
            if (opIndex == -1) return ParseExpression(expression, context, allowOverflow, lineNumber);
    
            string left = expression.Substring(0, opIndex).Trim();
            string right = expression.Substring(opIndex + 1).Trim();

            float leftVal = EvaluateExpression(left, context, lineNumber, allowOverflow);
            float rightVal = EvaluateExpression(right, context, lineNumber, allowOverflow);
            float result = operation(leftVal, rightVal);

            return result;
        }
    }
}