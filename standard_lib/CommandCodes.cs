namespace standard_lib
{
    public enum CommandCodes
    {
        InitHistory = 0x01,
        DeleteHistory = 0x02,
        DeleteCalibration = 0x03,

        /// <summary>
        ///     Params: 0x01/0x02 - On/Off
        /// </summary>
        LockDevice = 0x04,

        /// <summary>
        ///     Params: 0xXX - time in hours
        /// </summary>
        SetAutopause = 0x05,

        /// <summary>
        ///     Params: 0x01/0x02 - On/Off
        /// </summary>
        Pause = 0x06,

        /// <summary>
        ///     Answer will be [0xXX, 0xYY, 0xZZ, 0xZZ] where XX is Lock, YY is pause and ZZZZ is autopause time in minutes
        /// </summary>
        Status = 0x10,

        /// <summary>
        ///     Params: 0xXX, 0xZZ..0xZZ where XX is name length and ZZ is name chars. 19 chars max.
        /// </summary>
        ChangeName = 0x15,

        /// <summary>
        ///     Params: password in exactly 6 bytes
        /// </summary>
        ChangePassword = 0x30,

        /// <summary>
        ///     Corrupt device's firmware and reboot
        ///     Params: 0xAA
        /// </summary>
        ResetToBootloader = 0x50,

        /// <summary>
        ///     Params: 0x01/0x02 is Low/High speed
        /// </summary>
        ChangeConnectionSpeed = 0x60
    }
}