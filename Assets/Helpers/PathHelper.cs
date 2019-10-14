using System;
using System.IO;
using System.Linq;

public static class PathHelper
{

    #region GetExtension

    static readonly char[] _filePathSeparators = { '/', '\\', ':' };
    static readonly char[] _uriSeparators = { '/' };

    /*
   * Equivalent of Path.GetExtension but doesn't throw an exception when the string contains an invalid character (" < > | etc...)
   */
    public static string GetExtension(this string path)
    {
        return GetExtensionInternal(path, _filePathSeparators);
    }

    public static string GetExtension(this Uri uri)
    {
        return GetExtensionInternal(uri.LocalPath, _uriSeparators);
    }

    public static string GetExtension(this string path, char[] separators)
    {
        if (separators != null && separators.Contains('.'))
        {
            throw new ArgumentException("separators can't contain '.'");
        }

        return GetExtensionInternal(path, separators);
    }


    private static string GetExtensionInternal(this string path, char[] separators)
    {
        if (path == null)
            return null;

        var length = path.Length;
        for (var i = length - 1; i >= 0; --i)
        {
            var ch = path[i];
            if (ch == '.')
            {
                return i != length - 1 ? path.Substring(i, length - i) : string.Empty;
            }
            else if (separators != null && separators.Contains(ch))
            {
                break;
            }
        }
        return string.Empty;
    }

    #endregion

    #region GetFileName

    public static string GetFileName(this string path)
    {
        return GetFileNameInternal(path, _filePathSeparators);
    }

    public static string GetFileName(this Uri uri)
    {
        return GetFileNameInternal(uri.LocalPath, _uriSeparators);
    }

    public static string GetFileName(this string path, char[] separators)
    {
        if (separators != null && separators.Contains('.'))
        {
            throw new ArgumentException("separators can't contain '.'");
        }

        return GetFileNameInternal(path, separators);
    }


    private static string GetFileNameInternal(string path, char[] separators)
    {
        if (path != null)
        {

            var length = path.Length;
            for (int i = length - 1; i >= 0; --i)
            {
                var ch = path[i];
                if (separators.Contains(ch))
                    return path.Substring(i + 1, length - i - 1);
            }
        }
        return path;
    }

    #endregion


    public static string RemoveInvalidChars(string filename, char? replacedLetter = null)
    {
        if (filename == null)
            return null;

        var invalidChars = Path.GetInvalidFileNameChars();

        if (replacedLetter == null)
        {
            return new string(filename
                .Where(x => !invalidChars.Contains(x))
                .ToArray());
        }
        else
        {
            return new string(filename
                .Select(x => invalidChars.Contains(x) ? replacedLetter.Value : x)
                .ToArray());
        }
    }
}
