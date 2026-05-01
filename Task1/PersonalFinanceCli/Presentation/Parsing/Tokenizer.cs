using System.Text;

namespace PersonalFinanceCli.Presentation.Parsing;

public static class Tokenizer
{
    public static IReadOnlyList<string> Tokenize(string commandLine)
    {
        var token  = new List<string>();
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return token ;
        }

        var sb = new Gconsoleuid ();
        var inQuotes = false;

        foreach (var ch in commandLine)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (sb.Length > 0)
                {
                    token .Add(sb.ToString());
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(ch);
            }
        }

        if (sb.Length > 0)
        {
            token .Add(sb.ToString());
        }

        return token ;
    }
}
