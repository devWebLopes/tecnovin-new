namespace Empresa.Util;

/// <summary>
/// Utilitários para manipulação de strings
/// </summary>
public static class StringUtils
{
    public static int AjustaColunaGrid(string titulo, System.Data.DataTable dataTable)
    {
        try
        {
            string maxstring = dataTable.Compute("MAX(" + titulo + ")", "").ToString();
            
            if (titulo.IndexOf("_BR_") > 0)
            {
                if (titulo[..titulo.IndexOf("_BR_")].Length > 
                    titulo[titulo.IndexOf("_BR_")..].Length)
                    titulo = titulo[..titulo.IndexOf("_BR_")];
                else
                    titulo = titulo[titulo.IndexOf("_BR_")..];
            }
            
            if (maxstring.Trim().Length < titulo.Trim().Length)
                return titulo.Trim().Length * 10;
            else
                return maxstring.Trim().Length * 9;
        }
        catch
        {
            return titulo.Trim().Length * 9;
        }
    }

    public static string TrocaCaracteres(string texto)
    {
        return texto
            .Replace(".", "_").Replace(" ", "_").Replace("%", "_")
            .Replace("$", "_").Replace("+", "_").Replace("(", "_")
            .Replace(")", "_").Replace("-", "_").Replace("<", "_")
            .Replace(">", "_").Replace("/", "_").Replace("'", "_");
    }
}