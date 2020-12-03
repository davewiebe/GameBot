using System;
using System.Collections.Generic;
using System.Text;

namespace PerudoBot.Claims
{
    public static class ClaimParser
    {
        public static Claim Parse(string claimText)
        {
            var claimParts = claimText.ToLower().Trim().Split(" ");

            if (claimParts.Length != 2)
                throw new ArgumentException(ClaimExceptionMessages.InvalidClaimFormat);

            try
            {
                Operator op;
                if (claimParts[0].StartsWith('<'))
                {
                    op = Operator.LessThan;
                    claimParts[0] = claimParts[0].Trim('<');
                }
                else if (claimText.StartsWith('>'))
                {
                    op = Operator.GreaterThan;
                    claimParts[0] = claimParts[0].Trim('>');
                }
                else if (claimText.StartsWith('~'))
                {
                    op = Operator.Approximately;
                    claimParts[0] = claimParts[0].Trim('~');
                }
                else
                {
                    op = Operator.Exactly;
                }

                bool includeWilds = true;

                string[] wildExclusions = { "!", "nowilds" };
                foreach (var wildExclusion in wildExclusions)
                {
                    if (claimParts[1].EndsWith(wildExclusion))
                    {
                        includeWilds = false;
                        claimParts[1] = claimParts[1].Replace(wildExclusion, "");
                    }
                }

                // strip the s
                if (claimParts[1].EndsWith('s'))
                {
                    claimParts[1] = claimParts[1].Trim('s');
                }

                return new Claim(op, int.Parse(claimParts[0]), int.Parse(claimParts[1]), includeWilds);
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentException(ClaimExceptionMessages.InvalidClaimFormat);
            }
        }
    }
}