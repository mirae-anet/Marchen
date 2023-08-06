using System;

/// @brief 접속을 위해서 발급받는 connection token과 관련된 함수를 포함.
public static class ConnectionTokenUtils
{
    /// @brief Create new random Token
    public static byte[] NewToken() => Guid.NewGuid().ToByteArray();

    /// @brief Convert a Token into a Hash format
    /// @param token Token to be hashed
    /// @return Token hash
    public static int HashToken(byte[] token) => new Guid(token).GetHashCode();

    /// @breif Converts a Token into a String
    /// @param token Token to be parsed
    /// @return Token as a string
    public static string TokenToString(byte[] token) => new Guid(token).ToString();
}