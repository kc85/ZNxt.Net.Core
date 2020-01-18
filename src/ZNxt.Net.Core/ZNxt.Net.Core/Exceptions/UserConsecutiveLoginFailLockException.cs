using System;
using System.Collections.Generic;
using System.Text;

namespace ZNxt.Net.Core.Exceptions
{
    public class UserConsecutiveLoginFailLockException : Exception
    {
        /// <summary>
        /// Duration in millisecond  
        /// </summary>
        public double Duration { get; set; }
        public UserConsecutiveLoginFailLockException(double duration)
        {
            Duration = duration;
        }
    }
    public class UserConsecutiveLoginFailLockCountException : Exception
    {
        /// <summary>
        /// Duration in millisecond  
        /// </summary>
        public int RemainingCount { get; set; }
        public bool IsCapchaRequired { get; set; }
        public UserConsecutiveLoginFailLockCountException(int remainingCount, bool isCapchaRequired = false)
        {
            RemainingCount = remainingCount;
            IsCapchaRequired = isCapchaRequired;
        }
    }
}
