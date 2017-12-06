using System;

namespace CBANE.Sandpit
{
    public class NormalisedExampleRecord
    {
        public double Age;

        public double SpendCategoryA;
        public double SpendCategoryB;

        public bool PerformedAction;

        public NormalisedExampleRecord(ExampleRecord exampleRecord, int maxAge, double maxSpendCategoryA, double maxSpendCategoryB)
        {
            this.Age = Math.Round((double)exampleRecord.Age / maxAge, 6);

            this.SpendCategoryA = Math.Round((double)exampleRecord.SpendCategoryA / maxSpendCategoryA, 6);
            this.SpendCategoryB = Math.Round((double)exampleRecord.SpendCategoryB / maxSpendCategoryB, 6);
            
            this.PerformedAction = exampleRecord.PerformedAction;
        }
    }
}