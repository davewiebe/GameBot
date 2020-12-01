using NUnit.Framework;
using PerudoBot.Claims;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBotTests
{
    [TestFixture]
    public class ValidateClaimShould
    {
        [TestCase(Operator.Exactly, 1, 3, false, "1,2,3,4,5", true)]
        [TestCase(Operator.Exactly, 1, 3, true, "1,2,3,4,5", false)]
        [TestCase(Operator.Exactly, 1, 3, false, "1,2,2,2,3", true)]
        [TestCase(Operator.Exactly, 4, 2, true, "1,2,2,2,3", true)]
        [Test]
        public void ValidateExactClaims(Operator claimOperator, int claimQuantity, int claimPips, bool claimIncludeWilds, string diceText, bool expectedValidationResult)
        {
            var claim = new Claim(claimOperator, claimQuantity, claimPips, claimIncludeWilds);

            var isValid = ClaimValidator.Validate(claim, diceText);

            isValid.ShouldBe(expectedValidationResult);
        }

        [TestCase(Operator.Approximately, 2, 2, false, "2,3,4", true)]
        [TestCase(Operator.Approximately, 2, 2, false, "2,2,4", true)]
        [TestCase(Operator.Approximately, 2, 2, false, "2,2,2", true)]
        [TestCase(Operator.Approximately, 2, 2, false, "2,2,2,2", false)]
        [TestCase(Operator.Approximately, 2, 2, false, "1,1,1", false)]
        [Test]
        public void ValidateThatApproximateClaimsAreWithinOneOnEachSide(Operator claimOperator, int claimQuantity, int claimPips, bool claimIncludeWilds, string diceText, bool expectedValidationResult)
        {
            var claim = new Claim(claimOperator, claimQuantity, claimPips, claimIncludeWilds);

            var isValid = ClaimValidator.Validate(claim, diceText);

            isValid.ShouldBe(expectedValidationResult);
        }

        [TestCase(Operator.GreaterThan, 2, 2, true, "1,2,3,4", false)]
        [TestCase(Operator.GreaterThan, 2, 2, false, "2,2,4", false)]
        [TestCase(Operator.GreaterThan, 2, 2, false, "2,2,2", true)]
        [TestCase(Operator.GreaterThan, 2, 2, true, "1,1,1", true)]
        [Test]
        public void ValidateGreaterThanClaims(Operator claimOperator, int claimQuantity, int claimPips, bool claimIncludeWilds, string diceText, bool expectedValidationResult)
        {
            var claim = new Claim(claimOperator, claimQuantity, claimPips, claimIncludeWilds);

            var isValid = ClaimValidator.Validate(claim, diceText);

            isValid.ShouldBe(expectedValidationResult);
        }

        [TestCase(Operator.LessThan, 2, 2, true, "1,2,3,4", false)]
        [TestCase(Operator.LessThan, 2, 2, false, "2,2,4", false)]
        [TestCase(Operator.LessThan, 2, 2, false, "2,3,4", true)]
        [TestCase(Operator.LessThan, 2, 2, true, "1,1,1", false)]
        [TestCase(Operator.LessThan, 2, 2, true, "1,3,4", true)]
        [Test]
        public void ValidateLessThanClaims(Operator claimOperator, int claimQuantity, int claimPips, bool claimIncludeWilds, string diceText, bool expectedValidationResult)
        {
            var claim = new Claim(claimOperator, claimQuantity, claimPips, claimIncludeWilds);

            var isValid = ClaimValidator.Validate(claim, diceText);

            isValid.ShouldBe(expectedValidationResult);
        }
    }
}