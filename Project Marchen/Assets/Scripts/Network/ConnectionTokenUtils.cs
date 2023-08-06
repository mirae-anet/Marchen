using System;

/// @brief 접속을 위해서 발급받는 토큰을 관리함. 접속자를 식별하기 위해서 접속때마다 생성하는 토큰.
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