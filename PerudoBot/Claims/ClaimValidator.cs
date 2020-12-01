using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBot.Claims
{
    public static class ClaimValidator
    {
        public static bool Validate(Claim claim, string diceText)
        {
            var dice = diceText.Split(",")
                .Select(die => int.Parse(die));

            int actualQuantityOfClaimedPips;

            if (claim.IncludeWilds)
            {
                actualQuantityOfClaimedPips = dice.Where(die => die == claim.Pips || die == 1).Count();
            }
            else
            {
                actualQuantityOfClaimedPips = dice.Where(die => die == claim.Pips).Count();
            }

            return claim.Operator switch
            {
                Operator.GreaterThan => actualQuantityOfClaimedPips > claim.Quantity,
                Operator.LessThan => actualQuantityOfClaimedPips < claim.Quantity,
                Operator.Exactly => actualQuantityOfClaimedPips == claim.Quantity,
                Operator.Approximately => Enumerable.Range(claim.Quantity - 1, 3)
                                                .Contains(actualQuantityOfClaimedPips),
                _ => throw new ArgumentException("Claim has unrecognized operator"),
            };
        }
    }
}