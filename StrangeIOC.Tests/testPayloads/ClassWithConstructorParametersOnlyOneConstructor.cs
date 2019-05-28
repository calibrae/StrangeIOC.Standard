namespace strange.unittests
{
    public class ClassWithConstructorParametersOnlyOneConstructor
    {
        public ClassWithConstructorParametersOnlyOneConstructor(string stringVal)
        {
            this.stringVal = stringVal;
        }

        public string stringVal { get; }
    }
}