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
            return expression.Contains('|');
        }
    
        private static int ParseExpression(string expression, ParserContext context, bool allowOverflow = false)
        {
            /*
                hardcoded, only work with OR now, i dont think i will continue the support to other operators.
                I dont think they were useful for anyone
            */
            
            int opIndex = expression.LastIndexOf('|');
            if (opIndex == -1)
            {
                return EvaluateExpression(expression.Trim(), context, 0, allowOverflow);
            }

            string left = expression.Substring(0, opIndex).Trim();
            string right = expression.Substring(opIndex + 1).Trim();
            
            int leftVal = ParseExpression(left, context, allowOverflow);
            int rightVal = EvaluateExpression(right, context, 0, allowOverflow);
            int result = leftVal | rightVal;

            if (allowOverflow)
            {
                if (result < 0)
                {
                    while (result < 0) result += (byte.MaxValue+1);
                }
                return result % (byte.MaxValue+1);
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