using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApi.Utilities
{
    public static class ErrorType
    {
        public const string UnsupportedCode = "unsupported-code";

        public const string MalformedRequest = "malformed-request";

        public const string InvalidKey = "invalid-key";

        public const string InactiveAccount = "inactive-account";

        public const string QuotaReached = "quota-reached";

        public const string IncorrectHttpMethod = "incorrect-http-method";

        public const string PlanUpgradeRequired = "plan-upgrade-required";
    }
}
