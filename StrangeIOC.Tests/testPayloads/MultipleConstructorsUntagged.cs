namespace strange.unittests
{
    public class MultipleConstructorsUntagged
    {
        //Two constructors. One has three params, one has four
        public MultipleConstructorsUntagged(ISimpleInterface simple, int intValue, string stringValue)
        {
            this.simple = simple;
            this.intValue = intValue;
            this.stringValue = stringValue;
        }

        public MultipleConstructorsUntagged(ISimpleInterface simple, int intValue, string stringValue, float floatValue)
        {
            this.simple = simple;
            this.intValue = intValue;
            this.stringValue = stringValue;
            this.floatValue = floatValue;
        }

        public int intValue { get; set; }

        public float floatValue { get; set; }

        public string stringValue { get; set; }

        public ISimpleInterface simple { get; set; }
    }
}