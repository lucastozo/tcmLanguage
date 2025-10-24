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

                if (context.Macros.TryGetValue(expression, out string? macro))
                {
                    if (int.TryParse(macro, out int value))
                    {
                        return (byte)value;
                    }
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
    }
}