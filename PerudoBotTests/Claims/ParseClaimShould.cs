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
    public class ParseClaimShould
    {
        [TestCase("1 3!", Operator.Exactly, 1, 3, false)]
        [TestCase("1 3!", Operator.Exactly, 1, 3, false)]
        [TestCase("2 3s!", Operator.Exactly, 2, 3, false)]
        [TestCase("2 3nowilds", Operator.Exactly, 2, 3, false)]
        [Test]
        public void HandleClaimsWithNoOperatorsAndNoWilds(string claimText, Operator expectedOperator, int expectedQuantity, int expectedPips, bool expectedIncludeWilds)
        {
            var parsedClaim = ClaimParser.Parse(claimText);

            parsedClaim.Operator.ShouldBe(expectedOperator);
            parsedClaim.Quantity.ShouldBe(expectedQuantity);
            parsedClaim.Pips.ShouldBe(expectedPips);
            parsedClaim.IncludeWilds.ShouldBe(expectedIncludeWilds);
        }

        [TestCase("<1 3!", Operator.LessThan, 1, 3, false)]
        [TestCase("<2 4!", Operator.LessThan, 2, 4, false)]
        [TestCase("<4 6nowilds", Operator.LessThan, 4, 6, false)]
        [Test]
        public void HandleClaimsWithLessThanOperatorAndNoWilds(string claimText, Operator expectedOperator, int expectedQuantity, int expectedPips, bool expectedIncludeWilds)
        {
            var parsedClaim = ClaimParser.Parse(claimText);

            parsedClaim.Operator.ShouldBe(expectedOperator);
            parsedClaim.Quantity.ShouldBe(expectedQuantity);
            parsedClaim.Pips.ShouldBe(expectedPips);
            parsedClaim.IncludeWilds.ShouldBe(expectedIncludeWilds);
        }

        [TestCase(">1 3!", Operator.GreaterThan, 1, 3, false)]
        [TestCase(">3 4!", Operator.GreaterThan, 3, 4, false)]
        [Test]
        public void HandleClaimsWithGreaterThanOperatorAndNoWilds(string claimText, Operator expectedOperator, int expectedQuantity, int expectedPips, bool expectedIncludeWilds)
        {
            var parsedClaim = ClaimParser.Parse(claimText);

            parsedClaim.Operator.ShouldBe(expectedOperator);
            parsedClaim.Quantity.ShouldBe(expectedQuantity);
            parsedClaim.Pips.ShouldBe(expectedPips);
            parsedClaim.IncludeWilds.ShouldBe(expectedIncludeWilds);
        }

        [TestCase("~1 1", Operator.Approximately, 1, 1, true)]
        [TestCase("~4 3!", Operator.Approximately, 4, 3, false)]
        [Test]
        public void HandleClaimsWithTildeOperator(string claimText, Operator expectedOperator, int expectedQuantity, int expectedPips, bool expectedIncludeWilds)
        {
            var parsedClaim = ClaimParser.Parse(claimText);

            parsedClaim.Operator.ShouldBe(expectedOperator);
            parsedClaim.Quantity.ShouldBe(expectedQuantity);
            parsedClaim.Pips.ShouldBe(expectedPips);
            parsedClaim.IncludeWilds.ShouldBe(expectedIncludeWilds);
        }

        [TestCase("1 1", Operator.Exactly, 1, 1, true)]
        [TestCase("11 3", Operator.Exactly, 11, 3, true)]
        [Test]
        public void HandleClaimsThatIncludeWilds(string claimText, Operator expectedOperator, int expectedQuantity, int expectedPips, bool expectedIncludeWilds)
        {
            var parsedClaim = ClaimParser.Parse(claimText);

            parsedClaim.Operator.ShouldBe(expectedOperator);
            parsedClaim.Quantity.ShouldBe(expectedQuantity);
            parsedClaim.Pips.ShouldBe(expectedPips);
            parsedClaim.IncludeWilds.ShouldBe(expectedIncludeWilds);
        }

        [TestCase("1 1", Operator.Exactly, 1, 1, true)]
        [TestCase("11 3!", Operator.Exactly, 11, 3, false)]
        [TestCase(">3 4", Operator.GreaterThan, 3, 4, true)]
        [TestCase("<1 1!", Operator.LessThan, 1, 1, false)]
        [TestCase("~4 2!", Operator.Approximately, 4, 2, false)]
        [Test]
        public void HandleClaimsWithAnyOperatorAndVariableIncludeWilds(string claimText, Operator expectedOperator, int expectedQuantity, int expectedPips, bool expectedIncludeWilds)
        {
            var parsedClaim = ClaimParser.Parse(claimText);

            parsedClaim.Operator.ShouldBe(expectedOperator);
            parsedClaim.Quantity.ShouldBe(expectedQuantity);
            parsedClaim.Pips.ShouldBe(expectedPips);
            parsedClaim.IncludeWilds.ShouldBe(expectedIncludeWilds);
        }

        [TestCase("3 3 3")]
        [TestCase("43")]
        public void ThrowExceptionWhenInvalidClaimGiven(string claimText)
        {
            Should.Throw<ArgumentException>(() => ClaimParser.Parse(claimText), ClaimExceptionMessages.InvalidClaimFormat);
        }
    }
}