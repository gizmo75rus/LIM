namespace AstmDriver.Commons.ASTM
{
    public static class AstmHelpers
    {
        public static ResultAbnormalFlags GetFlag(string input)
        {
            switch (input)
            {
                case "L":
                    return ResultAbnormalFlags.BelowLowNormal;
                case "H":
                    return ResultAbnormalFlags.AboveHighNormal;
                case "LL":
                    return ResultAbnormalFlags.BelowPanicNormal;
                case "HH":
                    return ResultAbnormalFlags.AbovePanicHigh;
                case "<":
                    return ResultAbnormalFlags.BelowAbsolutelow;
                case ">":
                    return ResultAbnormalFlags.AboveAbsoluteHigh;
                case "N":
                    return ResultAbnormalFlags.Normal;
                case "A":
                    return ResultAbnormalFlags.Abnormal;
                case "U":
                    return ResultAbnormalFlags.SignificantChangeUp;
                case "D":
                    return ResultAbnormalFlags.SignificantChangeDown;
                case "B":
                    return ResultAbnormalFlags.Better;
                case "W":
                    return ResultAbnormalFlags.Worse;
                default:
                    return ResultAbnormalFlags.None;
            }
        }
    }
}
