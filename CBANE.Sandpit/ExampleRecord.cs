using System;

namespace CBANE.Sandpit
{
    public class ExampleRecord
    {
        public int Age;

        public double SpendCategoryA;
        public double SpendCategoryB;

        public bool PerformedAction;

        public ExampleRecord(int age, double spendCategoryA, double spendCategoryB, bool performedAction)
        {
            this.Age = age;

            this.SpendCategoryA = spendCategoryA;
            this.SpendCategoryB = spendCategoryB;
            
            this.PerformedAction = performedAction;
        }
    }
}